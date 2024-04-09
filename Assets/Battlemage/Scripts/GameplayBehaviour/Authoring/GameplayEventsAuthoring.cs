using System;
using System.Reflection;
using Battlemage.GameplayBehaviour.Data;
using Battlemage.GameplayBehaviour.Extensions;
using Battlemage.GameplayBehaviour.Utilities;
using BovineLabs.Core.Iterators;
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

                var methods = gameplayBehaviour.GetType()
                    .GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                var gameplayEventRefs = AddBuffer<GameplayEventReference>(entity);
                foreach (var method in methods)
                {
                    var attribute = method.GetCustomAttribute<GameplayEventAttribute>();
                    var delegateType = attribute.GameplayEventType.GetManagedType()
                        .GetCustomAttribute<GameplayEventDefinitionAttribute>().DelegateType;
                    var eventDelegate = Delegate.CreateDelegate(delegateType, method);
                    var componentType = attribute.GameplayEventType;

                    AddGameplayEvent(entity, gameplayBehaviour, componentType, eventDelegate, gameplayEventRefs);
                }
            }

            private void AddGameplayEvent(Entity entity, GameplayBehaviourAuthoring gameplayBehaviour,
                ComponentType componentType, Delegate eventDelegate,
                DynamicBuffer<GameplayEventReference> gameplayEventRefs)
            {
                var hash = GameplayBehaviourUtilities.GetEventHash(componentType);
                if (!TryGetBlobAssetReference<EventPointer>(hash, out var result))
                {
                    result = GameplayBehaviourUtilities.CreateEventPointerBlob(eventDelegate);
                    var blobMappingEntity = CreateAdditionalEntity(TransformUsageFlags.None);
                    AddComponent(blobMappingEntity, new GameplayEventBlobMapping
                    {
                        Hash = GameplayBehaviourUtilities.GetEventHash(gameplayBehaviour.GetType(), componentType,
                            eventDelegate.Method),
                        Pointer = result
                    });
                    AddBlobAssetWithCustomHash(ref result, hash);
                }

                AddComponent(entity, componentType);
                
                var localHash = componentType.IsComponent
                    ? GameplayBehaviourUtilities.GetEventHash(componentType)
                    : GameplayBehaviourUtilities.GetEventHash(componentType, eventDelegate.Method);

                gameplayEventRefs.Add(new GameplayEventReference()
                {
                    Hash = localHash,
                    EventPointerRef = result
                });
            }
        }
    }
}