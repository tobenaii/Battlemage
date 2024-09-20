using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Waddle.Runtime.GameplayAttributes;

namespace Waddle.Runtime.GameplayEffects
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup), OrderLast = true)]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
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
            public bool IsPermanentModification;
            public float Additive;
            public float Multiplicative;
            public float Division;
        }

        private NativeList<TargetAttributeKey> _keysToProcess;
        private NativeHashMap<TargetAttributeKey, AttributeModifications> _modificationsMap;
        
        public void OnCreate(ref SystemState state)
        {
            _keysToProcess = new NativeList<TargetAttributeKey>(20, Allocator.Persistent);
            _modificationsMap = new NativeHashMap<TargetAttributeKey, AttributeModifications>(20, Allocator.Persistent);
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

            foreach (var (gameplayEffect, attributeModifiers, entity) in SystemAPI
                         .Query<RefRW<GameplayEffect>, DynamicBuffer<GameplayAttributeModifier>>()
                         .WithAll<Simulate>()
                         .WithEntityAccess())
            {
                var target = gameplayEffect.ValueRO.Target;
                var duration = gameplayEffect.ValueRO.Duration;
                var isInstant = gameplayEffect.ValueRO.IsInstant;
                duration -= SystemAPI.Time.DeltaTime;
                if (duration <= 0)
                {
                    ecb.DestroyEntity(entity);
                }
                else
                {
                    gameplayEffect.ValueRW.Duration = duration;
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
                        case GameplayAttributeModifier.Operation.Subtract:
                            modifications.Additive -= modifier.Value;
                            break;
                        case GameplayAttributeModifier.Operation.Multiply:
                            modifications.Multiplicative *= modifier.Value;
                            break;
                        case GameplayAttributeModifier.Operation.Divide:
                            modifications.Division *= modifier.Value;
                            break;
                        case GameplayAttributeModifier.Operation.Override:
                            break;
                    }
                    _modificationsMap[key] = isInstant || duration > 0 ? modifications : default;
                }
            }
            var attributeBufferLookup = SystemAPI.GetBufferLookup<GameplayAttribute>();
            foreach (var key in _keysToProcess)
            {
                var modifications = _modificationsMap[key];
                var attributeBuffer = attributeBufferLookup[key.Target];
                var attribute = attributeBuffer[key.Attribute];
                var newValue = (attribute.BaseValue + modifications.Additive) * (1 + modifications.Multiplicative) /
                               (1 + modifications.Division);
                newValue = math.clamp(newValue, 0, float.MaxValue);
                attribute.CurrentValue = newValue;
                if (key.IsPermanent)
                {
                    attribute.BaseValue = newValue;
                }
                attributeBuffer[key.Attribute] = attribute;
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}