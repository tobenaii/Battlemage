using System;
using System.Reflection;
using System.Runtime.InteropServices;
using Battlemage.GameplayBehaviour.Data;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Hash128 = Unity.Entities.Hash128;

namespace Battlemage.GameplayBehaviour.Authoring
{
    [DisallowMultipleComponent, RequireComponent(typeof(GameplayBehaviourAuthoring))]
    public class GameplayEventsAuthoring : MonoBehaviour
    {
        public class GameplayEventsAuthoringBaker : Baker<GameplayEventsAuthoring>
        {
            public override void Bake(GameplayEventsAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                var gameplayBehaviour = GetComponent<GameplayBehaviourAuthoring>();
                DependsOn(gameplayBehaviour);
                AddComponent(entity, new GameplayBehaviourHash()
                {
                    Value = new Hash128(
                        (uint)gameplayBehaviour.GetType().GetHashCode(), 0, 0, 0)
                });
                foreach (var method in gameplayBehaviour.GetType()
                             .GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    var attribute = method.GetCustomAttribute<GameplayEventAttribute>();
                    var delegateType = attribute.GameplayEventType
                        .GetCustomAttribute<GameplayEventDefinitionAttribute>().DelegateType;
                    var eventDelegate = Delegate.CreateDelegate(delegateType, method);
                    var componentType = attribute.GameplayEventType;
                    var hash = new Hash128(
                        (uint)componentType.GetHashCode(),
                        (uint)gameplayBehaviour.GetType().GetHashCode(), 0, 0);
                    AddGameplayEvent(entity, eventDelegate, componentType, hash);
                }
            }

            private unsafe void AddGameplayEvent(Entity entity, Delegate eventDelegate, ComponentType componentType, Hash128 hash)
            {
                if (!TryGetBlobAssetReference<EventPointer>(hash, out var result))
                {
                    var builder = new BlobBuilder(Allocator.Temp);
                    ref var eventPointer = ref builder.ConstructRoot<EventPointer>();
                    eventPointer.Pointer = Marshal.GetFunctionPointerForDelegate(eventDelegate);
                    result = builder.CreateBlobAssetReference<EventPointer>(Allocator.Persistent);
                    builder.Dispose();
                    AddBlobAssetWithCustomHash(ref result, hash);
                    
                    var blobMappingEntity = CreateAdditionalEntity(TransformUsageFlags.None);
                    AddComponent(blobMappingEntity, new GameplayEventBlobMapping
                    {
                        Hash = hash,
                        Pointer = result
                    });
                }

                var resultPtr = new IntPtr(result.GetUnsafePtr());
                var handle = GCHandle.Alloc(resultPtr, GCHandleType.Pinned);
                try
                {
                    GetType().GetMethod("UnsafeAddComponent", BindingFlags.Instance | BindingFlags.NonPublic)!
                        .Invoke(this, new object[]
                        {
                            entity, componentType.TypeIndex, Marshal.SizeOf<EventPointer>(),
                            handle.AddrOfPinnedObject()
                        });
                }
                finally
                {
                    handle.Free();
                }
            }
        }
    }
}