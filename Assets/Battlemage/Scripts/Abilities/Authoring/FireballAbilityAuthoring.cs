using Battlemage.GameplayBehaviour.Authoring;
using Battlemage.GameplayBehaviour.Data;
using Unity.Burst;
using Unity.Entities;

namespace Battlemage.Abilities.Authoring
{
    [BurstCompile]
    public class FireballAbilityAuthoring : GameplayBehaviourAuthoring
    {
        public override GameplayOnHitEvent.Delegate OnHitCallback => OnHit;
        
        private static void OnHit(ref GameplayState state, ref Entity source, ref Entity target)
        {
            state.DealDamage(0.1f, target);
            UnityEngine.Debug.Log("Hello There");
            state.MarkForDestroy(source);
        }
    }
}