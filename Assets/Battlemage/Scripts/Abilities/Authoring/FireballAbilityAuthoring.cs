using Battlemage.GameplayBehaviours.Data;
using Battlemage.GameplayBehaviours.Data.GameplayEvents;
using Unity.Burst;
using Unity.Entities;
using Waddle.GameplayBehaviour.Authoring;
using Waddle.GameplayBehaviour.Data;

namespace Battlemage.Abilities.Authoring
{
    [BurstCompile]
    public class FireballAbilityAuthoring : GameplayBehaviourAuthoring
    {
        [GameplayEvent(typeof(GameplayOnSpawnEvent)), BurstCompile]
        private static void OnSpawn(ref GameplayState state, ref Entity self)
        {
            state.SetVelocity(self, state.GetForward(self) * 1.0f);
        }
        
        [GameplayEvent(typeof(GameplayOnHitEvent)), BurstCompile]
        private static void OnHit(ref GameplayState state, ref Entity self, ref Entity target)
        {
            state.DealDamage(self, 1.0f, target);
        }

        public class FireballAbilityAuthoringBaker : Baker<FireballAbilityAuthoring>
        {
            public override void Bake(FireballAbilityAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new Velocity.Data.Velocity());
            }
        }
    }
}