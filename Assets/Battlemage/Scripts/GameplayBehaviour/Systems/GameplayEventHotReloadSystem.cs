using System;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using Battlemage.GameplayBehaviour.Authoring;
using Battlemage.GameplayBehaviour.Data;
using Battlemage.GameplayBehaviour.Data.GameplayEvents;
using Battlemage.Utilities;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Unity.Entities;
using UnityEditor;
using UnityEngine;
using Hash128 = Unity.Entities.Hash128;

namespace Battlemage.GameplayBehaviour.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial class GameplayEventHotReloadSystem : SystemBase
    {
        private FileSystemWatcher _watcher;
        
        protected override void OnStartRunning()
        {
            _watcher = new FileSystemWatcher("Assets/Battlemage/Scripts/");
            _watcher.IncludeSubdirectories = true;
            _watcher.NotifyFilter = NotifyFilters.LastWrite;
            _watcher.Changed += (e, args) => EditorApplication.delayCall += () => Reload(args.FullPath.Replace('\\', '/').Replace(Application.dataPath, "Assets"));
            _watcher.EnableRaisingEvents = true;
        }

        protected override void OnStopRunning()
        {
            _watcher.Dispose();
        }

        private void Reload(string filePath)
        {
            var monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(filePath);
            if (monoScript == null || monoScript.GetClass().BaseType != typeof(GameplayBehaviourAuthoring))
            {
                return;
            }
            
            foreach (var method in monoScript.GetClass().GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            {
                var attribute = method.GetCustomAttribute<GameplayEventAttribute>();
                UpdateEventPointer(monoScript, attribute.GameplayEventType, method.Name);
            }
            Debug.Log($"Reloaded gameplay events: {monoScript.name}");
        }
        
        private void UpdateEventPointer(MonoScript script, Type eventType, string methodName)
        {
            var callback = CompileDelegate(script, methodName);
            BlobAssetReference<EventPointer> eventPointerBlob = default;
            var hash = new Hash128(
                (uint)eventType.GetHashCode(),
                (uint)script.GetClass().GetHashCode(), 0, 0);
            foreach (var blobMapping in SystemAPI.Query<GameplayEventBlobMapping>())
            {
                if (blobMapping.Hash == hash)
                {
                    eventPointerBlob = blobMapping.PointerBlobRef;
                    break;
                }
            }

            if (eventPointerBlob == default)
            {
                Debug.LogError($"Failed to find event pointer mapping for {script.name}: {methodName}");
                return;
            }
            eventPointerBlob.Value.Pointer = Marshal.GetFunctionPointerForDelegate(callback);
        }
        
        private static Delegate CompileDelegate(MonoScript script, string methodName)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(File.ReadAllText(AssetDatabase.GetAssetPath(script)));
            var root = syntaxTree.GetCompilationUnitRoot();

            var methodDeclaration = root
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .First(m => m.Identifier.ValueText == methodName && m.Modifiers.Any(SyntaxKind.StaticKeyword));

            var usingStatements = root.Usings.ToArray();

            MemberDeclarationSyntax memberDeclaration = SyntaxFactory.ClassDeclaration("Wrapper")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.StaticKeyword))
                .AddMembers(methodDeclaration);

            var compilationUnit = SyntaxFactory.CompilationUnit()
                .AddUsings(usingStatements)
                .AddMembers(memberDeclaration);
            
            var references = script.GetClass().Assembly.GetReferencedAssemblies()
                .Append(typeof(object).Assembly.GetName())
                .Append(typeof(GameplayBehaviourAuthoring).Assembly.GetName());
            var compilation = CSharpCompilation.Create("DynamicAssembly",
                    options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary))
                .AddReferences(references.Select(assemblyName => MetadataReference.CreateFromFile(Assembly.Load(assemblyName).Location)))
                .AddSyntaxTrees(compilationUnit.SyntaxTree);

            using var ms = new MemoryStream();
            var result = compilation.Emit(ms);

            if (result.Success)
            {
                ms.Seek(0, SeekOrigin.Begin);
                var assembly = Assembly.Load(ms.ToArray());
                var type = assembly.GetType("Wrapper");
                var methodInfo = type.GetMethod(methodName, BindingFlags.Static | BindingFlags.NonPublic);
                if (methodInfo != null)
                    return Delegate.CreateDelegate(MiscUtilities.GetDelegateType(methodInfo), methodInfo);
            }

            throw new InvalidOperationException("Compilation failed: " +
                                                string.Join(Environment.NewLine, result.Diagnostics));
        }
        
        protected override void OnUpdate()
        {
            
        }
    }
}