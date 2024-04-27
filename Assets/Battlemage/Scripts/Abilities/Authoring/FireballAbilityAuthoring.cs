using AOT;
using Battlemage.Attributes.Data;
using Battlemage.GameplayBehaviours.Data.GameplayEvents;
using Battlemage.GameplayBehaviours.Extensions;
using Battlemage.SimpleVelocity.Data;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine.Scripting;
using Waddle.GameplayAbilities.Data;
using Waddle.GameplayBehaviours.Authoring;
using Waddle.GameplayBehaviours.Data;
using Waddle.GameplayEffects.Data;
using Waddle.GameplayEffects.Extensions;

namespace Battlemage.Abilities.Authoring
{
    [BurstCompile, Preserve]
    public class FireballAbilityAuthoring : GameplayBehaviour
    {
        public class Baker : Baker<FireballAbilityAuthoring>
        {
            public override void Bake(FireballAbilityAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
                AddComponent(entity, new Velocity());
                
                AddComponent(entity, new GameplayAbilityData());
                AddBuffer<GameplayAbilityActivationAttributeRequirement>(entity);
            }
        }
        
        [GameplayEvent(typeof(GameplayOnSpawnEvent)), BurstCompile, Preserve, MonoPInvokeCallback(typeof(GameplayOnSpawnEvent.Delegate))]
        private static void OnSpawn(ref GameplayState state, ref Entity self)
        {
            var transform = state.GetComponent<LocalTransform>(self);
            transform.Position += transform.Forward(); 
            var velocity = new Velocity() { Value = transform.Forward() * 20.0f };
            
            state.SetComponent(self, velocity);
            state.SetComponent(self, transform);
            
            state.ScheduleEvent(self, 10.0f, nameof(DoExplode));
            state.AddOverlapSphereCallback(self, 0.25f, nameof(OnHit));
        }
        
        [GameplayEvent(typeof(GameplayOnHitEvent)), BurstCompile, Preserve, MonoPInvokeCallback(typeof(GameplayOnHitEvent.Delegate))]
        private static void OnHit(ref GameplayState state, ref Entity self, ref Entity target)
        {
            var abilityData = state.GetComponent<GameplayAbilityData>(self);
            if (target == abilityData.Source) return;
            state.CreateGameplayEffect()
                .WithAttributeModifier(
                    (byte)BattlemageAttribute.Health,
                    GameplayAttributeModifier.Operation.Negate,
                    10)
                .Apply(abilityData.Source, abilityData.Source);
            DoExplode(ref state, ref self);
        }
        
        [GameplayEvent(typeof(GameplayScheduledEvent)), BurstCompile, Preserve, MonoPInvokeCallback(typeof(GameplayOnHitEvent.Delegate))]
        private static void DoExplode(ref GameplayState state, ref Entity self)
        {
            state.MarkForDestroy(self);
        }
    }
}