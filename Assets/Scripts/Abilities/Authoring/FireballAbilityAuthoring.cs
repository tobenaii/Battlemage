using Battlemage.Attributes;
using Battlemage.GameplayBehaviours.GameplayEvents;
using Battlemage.SimpleVelocity;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using Waddle.Runtime.GameplayAbilities;
using Waddle.Runtime.GameplayBehaviours;
using Waddle.Runtime.GameplayEffects;

namespace Battlemage.Abilities.Authoring
{
    [BurstCompile]
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
        
        [GameplayEvent(typeof(GameplayOnSpawnEvent)), BurstCompile]
        private static void OnSpawn(ref GameplayState state, ref Entity self)
        {
            var transform = state.GetComponent<LocalTransform>(self);
            transform.Position += transform.Forward();
            state.SetComponent(self, transform);
            var velocity = new Velocity { Value = transform.Forward() * 20.0f };
            state.SetComponent(self, velocity);
            
            GameplayOnHitEvent.AddOnHitCallback(state, self, 0.25f, nameof(OnHit));
            GameplayScheduledEvent.Schedule(state, self, 5.0f, nameof(DoExplode));
        }
        
        [GameplayEvent(typeof(GameplayOnHitEvent)), BurstCompile]
        private static void OnHit(ref GameplayState state, ref Entity self, ref Entity target)
        {
            var abilityData = state.GetComponent<GameplayAbilityData>(self);
            if (target == abilityData.Source) return;
            state.Destroy(target);
            DoExplode(ref state, ref self);
        }
        
        [GameplayEvent(typeof(GameplayScheduledEvent)), BurstCompile]
        private static void DoExplode(ref GameplayState state, ref Entity self)
        {
            state.Destroy(self);
        }
    }
}