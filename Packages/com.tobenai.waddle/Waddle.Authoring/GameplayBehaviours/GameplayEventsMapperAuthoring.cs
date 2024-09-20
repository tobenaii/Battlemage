using System;
using System.Linq;
using System.Reflection;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Waddle.Runtime.GameplayBehaviours;
using Hash128 = Unity.Entities.Hash128;

namespace Waddle.Authoring.GameplayBehaviours
{
    public class GameplayEventsMapperAuthoring : MonoBehaviour
    {
        public class GameplayEventsMapperAuthoringBaker : Baker<GameplayEventsMapperAuthoring>
        {
            public override void Bake(GameplayEventsMapperAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<InitializeGameplayEvents>(entity);
                var gameplayEvents = AddBuffer<GameplayEventSetupData>(entity);
                
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();

                var behaviourTypes = assemblies.Select(assembly =>
                    assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(GameplayBehaviour)))).SelectMany(x => x);
                foreach (var behaviourType in behaviourTypes)
                {
                    var eventMethods = behaviourType
                        .GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                        .Where(x => x.GetCustomAttribute<GameplayEventAttribute>() != null);
                    foreach (var methodInfo in eventMethods)
                    {
                        var componentType = methodInfo.GetCustomAttribute<GameplayEventAttribute>()!.GameplayEventType;
                        var blobHash = new Hash128(
                            (uint)behaviourType.FullName!.GetHashCode(),
                            (uint)TypeManager.GetTypeInfo(componentType.TypeIndex).StableTypeHash.GetHashCode(),
                            0,
                            0);
                        
                        if (!TryGetBlobAssetReference<GameplayEventPointer>(blobHash, out var blobReference))
                        {
                            var builder = new BlobBuilder(Allocator.Temp);
                            ref var eventPointer = ref builder.ConstructRoot<GameplayEventPointer>();
                            eventPointer.Pointer = methodInfo.MethodHandle.GetFunctionPointer();
                            blobReference = builder.CreateBlobAssetReference<GameplayEventPointer>(Allocator.Persistent);
                            builder.Dispose();
                            AddBlobAssetWithCustomHash(ref blobReference, blobHash);
                        }
                        
                        gameplayEvents.Add(new GameplayEventSetupData
                        {
                            GameplayBehaviourHash = behaviourType.FullName.GetHashCode(),
                            EventHash = TypeManager.GetTypeInfo(componentType.TypeIndex).StableTypeHash,
                            Pointer = blobReference
                        });
                    }
                }
            }
        }
    }
}