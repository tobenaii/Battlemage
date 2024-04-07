using System;
using System.Runtime.InteropServices;
using Battlemage.GameplayBehaviour.Data;
using Battlemage.GameplayBehaviour.Data.GameplayEvents;
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
                if (gameplayBehaviour.OnSpawnEvent != default)
                {
                    AddGameplayEvent<GameplayOnSpawnEvent>(entity, gameplayBehaviour.OnSpawnEvent);
                }
                if (gameplayBehaviour.OnHitEvent != default)
                {
                    AddGameplayEvent<GameplayOnHitEvent>(entity, gameplayBehaviour.OnHitEvent);
                }
            }

            private void AddGameplayEvent<T>(Entity entity, Delegate del) where T : unmanaged, IGameplayEvent
            {
                var hash = new Hash128(
                    (uint)typeof(T).GetHashCode(),
                    (uint)del.GetHashCode(), 0, 0);
                if (!TryGetBlobAssetReference<EventPointer>(hash, out var result))
                {
                    var builder = new BlobBuilder(Allocator.Temp);
                    ref var eventPointer = ref builder.ConstructRoot<EventPointer>();
                    eventPointer.Pointer = Marshal.GetFunctionPointerForDelegate(del);
                    result = builder.CreateBlobAssetReference<EventPointer>(Allocator.Persistent);
                    builder.Dispose();
                    AddBlobAssetWithCustomHash(ref result, hash);
                }
                AddComponent(entity, new T()
                {
                    EventPointerRef = result
                });
            }
        }
    }
}