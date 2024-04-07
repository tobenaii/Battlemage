using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Battlemage.GameplayBehaviour.Authoring;
using Battlemage.GameplayBehaviour.Data;
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
            
            var syntaxTree = CSharpSyntaxTree.ParseText(File.ReadAllText(AssetDatabase.GetAssetPath(monoScript)));
            var root = syntaxTree.GetCompilationUnitRoot();
            var existingMethods = monoScript.GetClass()
                .GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            foreach (var method in existingMethods)
            {
                var attribute = method.GetCustomAttribute<GameplayEventAttribute>();
                var delegateType = attribute.GameplayEventType.GetCustomAttribute<GameplayEventDefinitionAttribute>().DelegateType;
                var eventDelegate = CompileDelegate(monoScript, delegateType, method.Name);
                var eventHash = new Hash128(
                    (uint)attribute.GameplayEventType.GetHashCode(),
                    (uint)monoScript.GetClass().GetHashCode(), 0, 0);
                
                ref var eventPointer = ref FindEventPointerByHash(eventHash);
                eventPointer.Pointer = Marshal.GetFunctionPointerForDelegate(eventDelegate);
            }
            Debug.Log($"Reloaded gameplay events: {monoScript.name}");
        }

        private ref EventPointer FindEventPointerByHash(Hash128 hash)
        {
            BlobAssetReference<EventPointer> eventPointerReference = default;
            foreach (var blobMapping in SystemAPI.Query<GameplayEventBlobMapping>())
            {
                if (blobMapping.Hash == hash)
                {
                    eventPointerReference = blobMapping.Pointer;
                    break;
                }
            }

            return ref eventPointerReference.Value;
        }
        
        private static Delegate CompileDelegate(MonoScript script, Type delegateType, string methodName)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(File.ReadAllText(AssetDatabase.GetAssetPath(script)));
            var root = syntaxTree.GetCompilationUnitRoot();

            var methodDeclaration = root
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .FirstOrDefault(m => m.Identifier.ValueText == methodName && m.Modifiers.Any(SyntaxKind.StaticKeyword));

            if (methodDeclaration == null)
            {
                var delegateMethod = delegateType.GetMethod("Invoke");
                var parameters = delegateMethod.GetParameters();

                var parameterList = SyntaxFactory.ParameterList(
                    SyntaxFactory.SeparatedList(
                        parameters.Select(p =>
                            SyntaxFactory.Parameter(SyntaxFactory.Identifier(p.Name))
                                .WithType(SyntaxFactory.ParseTypeName(p.ParameterType.FullName!.Replace("&", "")))
                                .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.RefKeyword)))
                            )
                    )
                );

                var returnType = SyntaxFactory.Token(SyntaxKind.VoidKeyword);

                methodDeclaration = SyntaxFactory.MethodDeclaration(SyntaxFactory.PredefinedType(returnType), SyntaxFactory.Identifier(methodName))
                    .WithModifiers(SyntaxFactory.TokenList(SyntaxFactory.Token(SyntaxKind.StaticKeyword)))
                    .WithParameterList(parameterList)
                    .WithBody(SyntaxFactory.Block());
            }

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
                    return Delegate.CreateDelegate(delegateType, methodInfo);
            }

            throw new InvalidOperationException("Compilation failed: " +
                                                string.Join(Environment.NewLine, result.Diagnostics));
        }
        
        protected override void OnUpdate()
        {
            
        }
    }
}