using Battlemage.GameplayBehaviour.Authoring;
using Battlemage.GameplayBehaviour.Data;
using Battlemage.GameplayBehaviour.Data.GameplayEvents;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

namespace Battlemage.Abilities.Authoring
{
    [BurstCompile]
    public class FireballAbilityAuthoring : GameplayBehaviourAuthoring
    {
        [GameplayEvent(typeof(GameplayOnSpawnEvent))]
        private static void OnSpawn(ref GameplayState state, ref Entity self)
        {
            state.SetVelocity(self, state.GetForward(self) * 1.0f);
            state.ScheduleEvent(self, 10.0f, DoExplode);
        }
        
        [GameplayEvent(typeof(GameplayOnHitEvent))]
        private static void OnHit(ref GameplayState state, ref Entity self, ref Entity target)
        {
            state.DealDamage(self, 10.0f, target);
            DoExplode(ref state, ref self);
        }
        
        [GameplayEvent(typeof(GameplayScheduledEvent))]
        private static void DoExplode(ref GameplayState state, ref Entity self)
        {
            Debug.Log("Fireball exploded!");
            state.MarkForDestroy(self);
        }
    }
}