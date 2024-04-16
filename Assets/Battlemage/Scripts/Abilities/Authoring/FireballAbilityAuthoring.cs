﻿using Battlemage.GameplayBehaviours.Data;
using Battlemage.GameplayBehaviours.Data.GameplayEvents;
using Unity.Entities;
using Waddle.GameplayBehaviour.Authoring;
using Waddle.GameplayBehaviour.Data;

namespace Battlemage.Abilities.Authoring
{
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
            DoExplode(ref state, ref self);
        }
        
        [GameplayEvent(typeof(GameplayScheduledEvent))]
        private static void DoExplode(ref GameplayState state, ref Entity self)
        {
            //Debug.Log("Fireball exploded!");
        }
    }
}