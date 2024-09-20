using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEditor;
using UnityEngine;
using Waddle.Runtime.GameplayBehaviours;

namespace Waddle.Authoring.GameplayBehaviours
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    public partial class GameplayEventHotReloadSystem : SystemBase
    {
        private FileSystemWatcher _watcher;
        private readonly Dictionary<int, List<MethodInfo>> _methodInfos = new();
        
        protected override void OnStartRunning()
        {
            _watcher = new FileSystemWatcher("Assets/Scripts/");
            _watcher.IncludeSubdirectories = true;
            _watcher.NotifyFilter = NotifyFilters.LastWrite;
            _watcher.Changed += (_, args) => EditorApplication.delayCall += () => Reload(args.FullPath.Replace('\\', '/').Replace(Application.dataPath, "Assets"));
            _watcher.EnableRaisingEvents = true;
        }

        protected override void OnStopRunning()
        {
            _watcher.Dispose();
        }

        private void Reload(string filePath)
        {
            var monoScript = AssetDatabase.LoadAssetAtPath<MonoScript>(filePath);
            if (monoScript == null || monoScript.GetClass().BaseType != typeof(GameplayBehaviour))
            {
                return;
            }

            var gameplayBehaviourHash = monoScript.GetClass().GetHashCode();
            if (!_methodInfos.TryGetValue(gameplayBehaviourHash, out var prevMethodInfos))
            {
                prevMethodInfos = monoScript.GetClass()
                    .GetMethods(BindingFlags.Static | BindingFlags.NonPublic)
                    .Where(x => x.GetCustomAttribute<GameplayEventAttribute>() != null)
                    .ToList();
            }
            
            var availableEvents = new List<(ComponentType type, ulong eventHash, Delegate eventDelegate)>();
            var eventSetupDataBuffer = SystemAPI.GetSingletonBuffer<GameplayEventSetupData>();
            var methodInfos = CompileEvents(monoScript).Where(x => x.GetCustomAttribute<GameplayEventAttribute>() != null).ToList();
            
            foreach (var methodInfo in methodInfos)
            {
                var attribute = methodInfo.GetCustomAttribute<GameplayEventAttribute>();
                var delegateType = attribute.GameplayEventType.GetManagedType().GetCustomAttribute<GameplayEventDefinitionAttribute>().DelegateType;
                var eventHash = TypeManager.GetTypeInfo(attribute.GameplayEventType.TypeIndex).StableTypeHash;
                var eventDelegate = Delegate.CreateDelegate(delegateType, methodInfo);
                availableEvents.Add((attribute.GameplayEventType, eventHash, eventDelegate));
                var newPointer = Marshal.GetFunctionPointerForDelegate(eventDelegate);
                
                var eventPointer = eventSetupDataBuffer.GetEventPointer(monoScript.GetClass(), attribute.GameplayEventType);
                eventPointer.Value.Pointer = newPointer;
            }
            
            var addedEvents = availableEvents
                .Where(availableEvent => !prevMethodInfos.Select(method => method.GetCustomAttribute<GameplayEventAttribute>().GameplayEventType).Contains(availableEvent.type)).ToList();
            
            var removedMethods = prevMethodInfos
                .Where(method =>
                    !availableEvents.Select(x => x.type).Contains(method.GetCustomAttribute<GameplayEventAttribute>()
                        .GameplayEventType))
                .ToList();
            
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach (var (behaviourHash, entity) in SystemAPI.Query<GameplayBehaviourHash>().WithEntityAccess().WithOptions(EntityQueryOptions.IncludePrefab))
            {
                if (behaviourHash.Value != gameplayBehaviourHash)
                {
                    continue;
                }

                foreach (var gEvent in addedEvents)
                {
                    if (!EntityManager.HasComponent(entity, gEvent.type))
                    {
                        var type = gEvent.type;
                        Debug.Log("Adding Component: " + type.GetManagedType().Name);
                        ecb.AddComponent(entity, gEvent.type);
                    }
                }
                
                foreach (var method in removedMethods)
                {
                    var type = method.GetCustomAttribute<GameplayEventAttribute>().GameplayEventType;
                    if (type.IsComponent && EntityManager.HasComponent(entity, type))
                    {
                        Debug.Log("Removing Component: " + type.GetManagedType().Name);
                        ecb.RemoveComponent(entity, type);
                    }
                }
            }
            
            ecb.Playback(EntityManager);
            ecb.Dispose();
            _methodInfos[gameplayBehaviourHash] = methodInfos;
            Debug.Log($"Reloaded {(World.IsServer() ? "server" : "client")} gameplay events: {monoScript.name}");
        }
        
        private static IEnumerable<MethodInfo> CompileEvents(MonoScript script)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(File.ReadAllText(AssetDatabase.GetAssetPath(script)));
            var root = syntaxTree.GetCompilationUnitRoot();

            var rootNamespace = root.Members.OfType<NamespaceDeclarationSyntax>().FirstOrDefault();
            var usingStatements = root.Usings.ToArray();
            
            var methodDeclarations = root
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Where(m => m.Modifiers.Any(SyntaxKind.StaticKeyword))
                .Select(method => (MemberDeclarationSyntax)method)
                .ToArray();

            MemberDeclarationSyntax memberDeclaration = SyntaxFactory.NamespaceDeclaration(rootNamespace!.Name)
                .AddMembers(SyntaxFactory.ClassDeclaration("Wrapper")
                    .AddModifiers(SyntaxFactory.Token(SyntaxKind.StaticKeyword))
                    .AddMembers(methodDeclarations));

            var compilationUnit = SyntaxFactory.CompilationUnit()
                .AddUsings(usingStatements)
                .AddMembers(memberDeclaration);
            
            var references = script.GetClass().Assembly.GetReferencedAssemblies()
                .Append(typeof(object).Assembly.GetName())
                .Append(script.GetClass().Assembly.GetName());
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
                var type = assembly.GetType($"{rootNamespace.Name}.Wrapper");
                var methodInfos = type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic);
                return methodInfos;
            }

            throw new InvalidOperationException("Compilation failed: " +
                                                string.Join(Environment.NewLine, result.Diagnostics));
        }
        
        protected override void OnUpdate()
        {
            
        }
    }
}