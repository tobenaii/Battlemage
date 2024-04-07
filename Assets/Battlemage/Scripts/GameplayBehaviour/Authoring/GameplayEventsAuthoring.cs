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
                if (gameplayBehaviour.OnHitCallback != default)
                {
                    if (!TryGetBlobAssetReference<EventPointer>(new Hash128("OnHit"), out var result))
                    {
                        var builder = new BlobBuilder(Allocator.Temp);
                        ref var eventPointer = ref builder.ConstructRoot<EventPointer>();
                        eventPointer.Pointer = Marshal.GetFunctionPointerForDelegate(gameplayBehaviour.OnHitCallback);
                        result = builder.CreateBlobAssetReference<EventPointer>(Allocator.Persistent);
                        builder.Dispose();
                        AddBlobAssetWithCustomHash(ref result, new Hash128("OnHit"));
                    }
                    AddComponent(entity, new GameplayOnHitEvent()
                    {
                        EventPointerRef = result
                    });
                }
            }
        }
    }
}