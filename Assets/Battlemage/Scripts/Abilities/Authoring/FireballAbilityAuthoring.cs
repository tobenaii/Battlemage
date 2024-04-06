using AOT;
using Battlemage.GameplayBehaviour.Authoring;
using Battlemage.GameplayBehaviour.Data;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

namespace Battlemage.Abilities.Authoring
{
    [BurstCompile]
    public class FireballAbilityAuthoring : GameplayBehaviourAuthoring
    {
        public override GameplayOnHitCallback.Delegate OnHitCallback => OnHit;

        [BurstCompile]
        [MonoPInvokeCallback(typeof(GameplayOnHitCallback.Delegate))]
        private static void OnHit(ref SystemState state, ref Entity ability, ref Entity target)
        {
            Debug.Log("Hello there.\nGeneral Kenobi");
        }
    }
}