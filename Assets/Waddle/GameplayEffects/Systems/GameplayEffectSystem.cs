using System;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using Waddle.GameplayAttributes.Data;
using Waddle.GameplayAttributes.Extensions;
using Waddle.GameplayEffects.Data;

namespace Waddle.GameplayEffects.Systems
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup), OrderLast = true)]
    [CreateAfter(typeof(DefaultVariantSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    public partial struct GameplayEffectSystem : ISystem
    {
        private struct TargetAttributeKey : IEquatable<TargetAttributeKey>
        {
            public Entity Target;
            public byte Attribute;
            public bool IsPermanent;

            public bool Equals(TargetAttributeKey other)
            {
                return Target.Equals(other.Target) && Attribute.Equals(other.Attribute) && IsPermanent == other.IsPermanent;
            }
        }
        private struct AttributeModifications
        {
            public bool isPermanentModification;
            public float Additive;
            public float Multiplicitive;
            public float Division;
        }

        private NativeList<TargetAttributeKey> _keysToProcess;
        private NativeHashMap<TargetAttributeKey, AttributeModifications> _modificationsMap;
        
        public void OnCreate(ref SystemState state)
        {
            _keysToProcess = new NativeList<TargetAttributeKey>(20, Allocator.Persistent);
            _modificationsMap = new NativeHashMap<TargetAttributeKey, AttributeModifications>(20, Allocator.Persistent);
            
            var prefab = state.EntityManager.CreateEntity(
                ComponentType.ReadOnly<Prefab>(),
                ComponentType.ReadOnly<GhostOwner>(),
                ComponentType.ReadOnly<GameplayEffect>(),
                ComponentType.ReadOnly<GameplayAttributeModifier>(),
                ComponentType.ReadOnly<GameplayTagModifier>()
            );

            GhostPrefabCreation.ConvertToGhostPrefab(state.EntityManager, prefab, new GhostPrefabCreation.Config()
            {
                Name = "GameplayEffect",
                DefaultGhostMode = GhostMode.OwnerPredicted,
            });
            
            state.EntityManager.CreateSingleton(new GameplayEffectPrefab()
            {
                Value = prefab
            });
            
            state.RequireForUpdate<NetworkTime>();
        }

        public void OnDestroy(ref SystemState state)
        {
            _keysToProcess.Dispose();
            _modificationsMap.Dispose();
        }
        
        public void OnUpdate(ref SystemState state)
        {
            _keysToProcess.Clear();
            _modificationsMap.Clear();

            var ecb = new EntityCommandBuffer(Allocator.Temp);
            var predictedGhostLookup = SystemAPI.GetComponentLookup<PredictedGhost>();

            foreach (var (gameplayEffect, attributeModifiers, tagModifiers, predictedGhost, entity) in SystemAPI
                         .Query<RefRW<GameplayEffect>, DynamicBuffer<GameplayAttributeModifier>,
                             DynamicBuffer<GameplayTagModifier>, RefRO<PredictedGhost>>()
                         .WithAll<Simulate>()
                         .WithEntityAccess())
            {
                var characterGhost = predictedGhostLookup[gameplayEffect.ValueRO.Target];
                var tick = predictedGhost.ValueRO.AppliedTick;
                if (tick.IsValid)
                {
                    tick.Decrement();
                    if (characterGhost.AppliedTick.IsNewerThan(tick))
                    {
                        continue;
                    }
                }
                var target = gameplayEffect.ValueRO.Target;
                var isInstant = gameplayEffect.ValueRO.Duration == 0 && state.WorldUnmanaged.IsServer();
                if (isInstant)
                {
                    ecb.DestroyEntity(entity);
                }
                foreach (var modifier in attributeModifiers)
                {
                    var key = new TargetAttributeKey()
                    {
                        Target = target,
                        Attribute = modifier.ModAttribute,
                        IsPermanent = isInstant,
                    };
                    if (!_keysToProcess.Contains(key))
                    {
                        _keysToProcess.Add(key);
                    }

                    _modificationsMap.TryGetValue(key, out var modifications);
                    switch (modifier.OperationType)
                    {
                        case GameplayAttributeModifier.Operation.Add:
                            modifications.Additive += modifier.Value;
                            break;
                        case GameplayAttributeModifier.Operation.Negate:
                            modifications.Additive -= modifier.Value;
                            break;
                        case GameplayAttributeModifier.Operation.Multiply:
                            modifications.Multiplicitive *= modifier.Value;
                            break;
                        case GameplayAttributeModifier.Operation.Divide:
                            modifications.Division *= modifier.Value;
                            break;
                        case GameplayAttributeModifier.Operation.Override:
                            break;
                    }
                    _modificationsMap[key] = modifications;
                }
            }
            var attributeBufferLookup = SystemAPI.GetBufferLookup<GameplayAttributeMap>();
            foreach (var key in _keysToProcess)
            {
                if (!key.IsPermanent) continue;
                var modifications = _modificationsMap[key];
                var attributeBuffer = attributeBufferLookup[key.Target].AsMap();
                var attribute = attributeBuffer[key.Attribute];
                var newValue = (attribute.BaseValue + modifications.Additive) * (1 + modifications.Multiplicitive) /
                               (1 + modifications.Division);
                attribute.CurrentValue = newValue;
                attribute.BaseValue = newValue;
                attributeBuffer[key.Attribute] = attribute;
            }
            
            foreach (var key in _keysToProcess)
            {
                if (key.IsPermanent) continue;
                
                var modifications = _modificationsMap[key];
                var attributeBuffer = attributeBufferLookup[key.Target].AsMap();
                var attribute = attributeBuffer[key.Attribute];
                var newValue = (attribute.BaseValue + modifications.Additive) * (1 + modifications.Multiplicitive) /
                               (1 + modifications.Division);
                attribute.CurrentValue = newValue;
                attributeBuffer[key.Attribute] = attribute;
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}