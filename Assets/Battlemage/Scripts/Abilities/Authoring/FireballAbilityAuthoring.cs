using Battlemage.GameplayBehaviour.Authoring;
using Battlemage.GameplayBehaviour.Data;
using Battlemage.GameplayBehaviour.Data.GameplayEvents;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;

namespace Battlemage.Abilities.Authoring
{
    [BurstCompile]
    public class FireballAbilityAuthoring : GameplayBehaviourAuthoring
    {
        [GameplayEvent(typeof(GameplayOnSpawnEvent))]
        private static void OnSpawn(ref GameplayState state, ref Entity self)
        {
            state.SetVelocity(self, new float3(1, 0, 0));
        }
        
        [GameplayEvent(typeof(GameplayOnHitEvent))]
        private static void OnHit(ref GameplayState state, ref Entity self, ref Entity target)
        {
            state.DealDamage(0.1f, target);
            state.MarkForDestroy(self);
        }
    }
}