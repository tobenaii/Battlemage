using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Battlemage.GameplayBehaviour.Authoring;
using Battlemage.GameplayBehaviour.Data;
using Battlemage.GameplayBehaviour.Data.GameplayEvents;
using Battlemage.GameplayBehaviour.Utilities;
using BovineLabs.Core.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Unity.Collections;
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
        private Dictionary<Hash128, List<MethodInfo>> _methodInfos = new();
        
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
            
            var monoHash = new Hash128((uint)monoScript.GetClass().GetHashCode(), 0, 0, 0);
            if (!_methodInfos.TryGetValue(monoHash, out var prevMethodInfos))
            {
                prevMethodInfos = monoScript.GetClass().GetMethods(BindingFlags.Static | BindingFlags.NonPublic).ToList();
            }
            var methodInfos = CompileEvents(monoScript).ToList();
            var availableEvents = new List<(ComponentType type, Hash128 hash, Delegate eventDelegate)>();
            foreach (var methodInfo in methodInfos)
            {
                var attribute = methodInfo.GetCustomAttribute<GameplayEventAttribute>();
                var delegateType = attribute.GameplayEventType.GetManagedType().GetCustomAttribute<GameplayEventDefinitionAttribute>().DelegateType;
                var eventHash = GameplayBehaviourUtilities.GetEventHash(attribute.GameplayEventType, monoScript.GetClass(), methodInfo);
                var eventPointerBlob = GameplayBehaviourUtilities.FindEventPointerByHash(EntityManager, eventHash);
                var eventDelegate = Delegate.CreateDelegate(delegateType, methodInfo);
                availableEvents.Add((attribute.GameplayEventType, eventHash, eventDelegate));
                if (!eventPointerBlob.IsCreated)
                {
                    continue;
                }
                eventPointerBlob.Value.Pointer = Marshal.GetFunctionPointerForDelegate(eventDelegate);
            }

            var removedMethods = prevMethodInfos
                .Where(method =>
                    !availableEvents.Select(x => x.type).Contains(method.GetCustomAttribute<GameplayEventAttribute>()
                        .GameplayEventType))
                .ToList();
            
            var addedEventTypes = availableEvents
                .Where(availableEvent => !prevMethodInfos.Select(method => method.GetCustomAttribute<GameplayEventAttribute>().GameplayEventType).Contains(availableEvent.type)).ToList();
            
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach (var (behaviourHash, entity) in SystemAPI.Query<GameplayBehaviourHash>().WithEntityAccess().WithOptions(EntityQueryOptions.IncludePrefab))
            {
                if (behaviourHash.Value != monoHash)
                {
                    continue;
                }

                foreach (var method in removedMethods)
                {
                    var eventAttribute = method.GetCustomAttribute<GameplayEventAttribute>();
                    var eventType = eventAttribute.GameplayEventType;
                    if (eventType.IsComponent && EntityManager.HasComponent(entity, eventType))
                    {
                        ecb.RemoveComponent(entity, eventType);
                    }
                    else if (eventType.IsBuffer)
                    {
                        var buffer = EntityManager.GetBuffer<GameplayScheduledEvent>(entity);
                        var hash = GameplayBehaviourUtilities.GetEventHash(eventType, monoScript.GetType(), method);
                        buffer.RemoveAt(buffer.IndexOf(x => x.EventPointerRef == GameplayBehaviourUtilities.FindEventPointerByHash(EntityManager, hash)));
                    }
                }

                foreach (var gEvent in addedEventTypes)
                {
                    if (!EntityManager.HasComponent(entity, gEvent.type))
                    {
                        AddGameplayEvent(entity, gEvent.eventDelegate, gEvent.type, gEvent.hash, ecb);
                    }
                }
            }
            ecb.Playback(EntityManager);
            ecb.Dispose();
            _methodInfos[monoHash] = methodInfos;
            Debug.Log($"Reloaded gameplay events: {monoScript.name}");
        }
        
        private unsafe void AddGameplayEvent(Entity entity, Delegate eventDelegate, ComponentType componentType, Hash128 hash, EntityCommandBuffer ecb)
        {
            if (!componentType.IsComponent)
                return;
            var eventPointerReference = GameplayBehaviourUtilities.FindEventPointerByHash(EntityManager, hash);
            if (!eventPointerReference.IsCreated)
            {
                eventPointerReference = GameplayBehaviourUtilities.CreateEventPointerBlob(eventDelegate);
                    
                var blobMappingEntity = ecb.CreateEntity();
                ecb.AddComponent(blobMappingEntity, new GameplayEventBlobMapping
                {
                    Hash = hash,
                    Pointer = eventPointerReference
                });
            }

            var resultPtr = new IntPtr(eventPointerReference.GetUnsafePtr());
            var handle = GCHandle.Alloc(resultPtr, GCHandleType.Pinned);
            try
            {
                ecb.UnsafeAddComponent(entity, componentType.TypeIndex, Marshal.SizeOf<EventPointer>(), handle.AddrOfPinnedObject().ToPointer());
            }
            finally
            {
                handle.Free();
            }
        }
        
        private static IEnumerable<MethodInfo> CompileEvents(MonoScript script)
        {
            var syntaxTree = CSharpSyntaxTree.ParseText(File.ReadAllText(AssetDatabase.GetAssetPath(script)));
            var root = syntaxTree.GetCompilationUnitRoot();

            var usingStatements = root.Usings.ToArray();
            
            var methodDeclarations = root
                .DescendantNodes()
                .OfType<MethodDeclarationSyntax>()
                .Where(m => m.Modifiers.Any(SyntaxKind.StaticKeyword));

            MemberDeclarationSyntax memberDeclaration = SyntaxFactory.ClassDeclaration("Wrapper")
                .AddModifiers(SyntaxFactory.Token(SyntaxKind.StaticKeyword))
                .AddMembers(methodDeclarations.ToArray());

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