using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Battlemage.GameplayBehaviour.Authoring;
using Battlemage.GameplayBehaviour.Data;
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
                var delegateType = attribute.GameplayEventType.GetCustomAttribute<GameplayEventDefinitionAttribute>().DelegateType;
                var eventHash = new Hash128(
                    (uint)attribute.GameplayEventType.GetHashCode(),
                    (uint)monoScript.GetClass().GetHashCode(), 0, 0);
                var eventPointerBlob = FindEventPointerByHash(eventHash);
                var eventDelegate = Delegate.CreateDelegate(delegateType, methodInfo);
                availableEvents.Add((attribute.GameplayEventType, eventHash, eventDelegate));
                if (!eventPointerBlob.IsCreated)
                {
                    continue;
                }
                eventPointerBlob.Value.Pointer = Marshal.GetFunctionPointerForDelegate(eventDelegate);
            }

            var removedEventTypes = prevMethodInfos
                .Where(method =>
                    !availableEvents.Select(x => x.type).Contains(method.GetCustomAttribute<GameplayEventAttribute>()
                        .GameplayEventType))
                .Select(method => method.GetCustomAttribute<GameplayEventAttribute>().GameplayEventType).ToList();
            
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach (var (behaviourHash, entity) in SystemAPI.Query<GameplayBehaviourHash>().WithEntityAccess())
            {
                if (behaviourHash.Value != monoHash)
                {
                    continue;
                }

                foreach (var eventType in removedEventTypes)
                {
                    if (EntityManager.HasComponent(entity, eventType))
                    {
                        ecb.RemoveComponent(entity, eventType);
                    }
                }

                foreach (var gEvent in availableEvents)
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
            var eventPointerReference = FindEventPointerByHash(hash);
            if (!eventPointerReference.IsCreated)
            {
                var builder = new BlobBuilder(Allocator.Temp);
                ref var eventPointer = ref builder.ConstructRoot<EventPointer>();
                eventPointer.Pointer = Marshal.GetFunctionPointerForDelegate(eventDelegate);
                eventPointerReference = builder.CreateBlobAssetReference<EventPointer>(Allocator.Persistent);
                builder.Dispose();
                    
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

        private BlobAssetReference<EventPointer> FindEventPointerByHash(Hash128 hash)
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

            return eventPointerReference;
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