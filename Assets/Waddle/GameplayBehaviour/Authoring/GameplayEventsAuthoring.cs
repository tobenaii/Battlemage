using System;
using System.Linq;
using System.Reflection;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Waddle.GameplayActions.Data;
using Waddle.GameplayBehaviour.Data;
using Hash128 = Unity.Entities.Hash128;

namespace Waddle.GameplayBehaviour.Authoring
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
                    .GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(x => x.GetCustomAttribute<GameplayEventAttribute>() != null);
                var fullGameplayEventRefs = AddBuffer<FullGameplayEventReference>(entity);
                foreach (var method in methods.Where(x => !x.Name.Contains("$BurstManaged")))
                {
                    var attribute = method.GetCustomAttribute<GameplayEventAttribute>();
                    var delegateType = attribute.GameplayEventType.GetManagedType()
                        .GetCustomAttribute<GameplayEventDefinitionAttribute>().DelegateType;
                    var eventDelegate = Delegate.CreateDelegate(delegateType, method);
                    var componentType = attribute.GameplayEventType;

                    AddGameplayEvent(entity, gameplayBehaviour.GetType(), componentType, eventDelegate, fullGameplayEventRefs);
                }

                AddBuffer<GameplayActionRequirement>(entity);
            }

            private void AddGameplayEvent(Entity entity, Type behaviourType,
                ComponentType componentType, Delegate eventDelegate,
                DynamicBuffer<FullGameplayEventReference> fullGameplayEventRefs)
            {
                var eventInfoEntity = CreateAdditionalEntity(TransformUsageFlags.None, true);
                AddComponent(eventInfoEntity, new GameplayEventInfo
                {
                    AssemblyQualifiedName = behaviourType.AssemblyQualifiedName,
                    MethodName = eventDelegate.Method.Name,
                });

                AddComponent(entity, componentType);

                var typeHash = TypeManager.GetTypeInfo(componentType.TypeIndex).StableTypeHash;
                var methodHash = componentType.IsBuffer ? new FixedString64Bytes(eventDelegate.Method.Name).GetHashCode() : 0;
                
                var fullHash = new Hash128(
                    (uint)behaviourType.AssemblyQualifiedName!.GetHashCode(),
                    (uint)eventDelegate.Method.Name.GetHashCode(), 0, 0);

                fullGameplayEventRefs.Add(new FullGameplayEventReference()
                {
                    TypeHash = typeHash,
                    MethodHash = methodHash,
                    FullHash = fullHash
                });
            }
        }
    }
}