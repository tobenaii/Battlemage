using AOT;
using Battlemage.GameplayBehaviour.Authoring;
using Battlemage.GameplayBehaviour.Data;
using Unity.Burst;
using Unity.Entities;

namespace Battlemage.Abilities.Authoring
{
    [BurstCompile]
    public class FireballAbilityAuthoring : GameplayBehaviourAuthoring
    {
        public override GameplayOnHitCallback.Delegate OnHitCallback => OnHit;

        [BurstCompile]
        [MonoPInvokeCallback(typeof(GameplayOnHitCallback.Delegate))]
        private static void OnHit(ref GameplayState state, ref Entity source, ref Entity target)
        {
            state.DealDamage(10.0f, target);
            state.MarkForDestroy(source);
        }
    }
}