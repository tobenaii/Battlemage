using System;
using System.Linq;
using System.Reflection;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Waddle.GameplayActions.Data;
using Waddle.GameplayBehaviours.Data;
using Hash128 = Unity.Entities.Hash128;

namespace Waddle.GameplayBehaviours.Authoring
{
    [DisallowMultipleComponent, RequireComponent(typeof(GameplayBehaviour))]
    public class GameplayBehaviourAuthoring : MonoBehaviour
    {
        public class Baker : Baker<GameplayBehaviourAuthoring>
        {
            public override void Bake(GameplayBehaviourAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                var gameplayBehaviour = GetComponent<GameplayBehaviour>();
                var gameplayEvents = AddBuffer<GameplayEventReference>(entity);
                var methods = gameplayBehaviour.GetType()
                    .GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(x => x.GetCustomAttribute<GameplayEventAttribute>() != null);
                
                foreach (var method in methods)
                {
                    var attribute = method.GetCustomAttribute<GameplayEventAttribute>();
                    var componentType = attribute.GameplayEventType;
                    AddGameplayEvent(entity, gameplayBehaviour.GetType(), method, componentType, gameplayEvents);
                }

                AddBuffer<GameplayActionRequirement>(entity);
                AddComponent(entity, new GameplayBehaviourHash()
                {
                    Value = authoring.GetType().GetHashCode()
                });
            }

            private void AddGameplayEvent(Entity entity, Type gameplayType, MethodInfo methodInfo,
                ComponentType componentType, DynamicBuffer<GameplayEventReference> gameplayEvents)
            {
                AddComponent(entity, componentType);

                var blobHash = new Hash128(
                    (uint)gameplayType.FullName!.GetHashCode(),
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
                
                gameplayEvents.Add(new GameplayEventReference()
                {
                    EventHash = TypeManager.GetTypeInfo(componentType.TypeIndex).StableTypeHash,
                    MethodName = methodInfo.Name,
                    Pointer = blobReference
                });
            }
        }
    }
}