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
        public override GameplayOnSpawnEvent.Delegate OnSpawnEvent => OnSpawn;
        public override GameplayOnHitEvent.Delegate OnHitEvent => OnHit;
        
        private static void OnSpawn(ref GameplayState state, ref Entity source)
        {
            state.SetVelocity(source, new float3(1, 0, 0));
        }
        
        private static void OnHit(ref GameplayState state, ref Entity source, ref Entity target)
        {
            state.DealDamage(0.1f, target);
            UnityEngine.Debug.Log("Hello There");
            state.MarkForDestroy(source);
        }
    }
}