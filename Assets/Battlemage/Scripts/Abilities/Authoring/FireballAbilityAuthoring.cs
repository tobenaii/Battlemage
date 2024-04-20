using System.Runtime.InteropServices;
using AOT;
using Battlemage.GameplayBehaviours.Data;
using Battlemage.GameplayBehaviours.Data.GameplayEvents;
using Unity.Burst;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Scripting;
using Waddle.Abilities.Data;
using Waddle.GameplayBehaviour.Authoring;
using Waddle.GameplayBehaviour.Data;

namespace Battlemage.Abilities.Authoring
{
    [BurstCompile, Preserve]
    public class FireballAbilityAuthoring : GameplayBehaviourAuthoring
    {
        [GameplayEvent(typeof(GameplayOnSpawnEvent)), BurstCompile, Preserve, MonoPInvokeCallback(typeof(GameplayOnSpawnEvent.Delegate))]
        private static void OnSpawn(ref GameplayState state, ref Entity self)
        {
            var abilityData = state.GetComponent<AbilityData>(self);
            var playerTransform = state.GetComponent<LocalTransform>(abilityData.Source);
            playerTransform.Position += playerTransform.Up() * 1.25f;
            state.SetComponent(self, playerTransform);
            state.SetVelocity(self, state.GetForward(self) * 20.0f);
            
            state.ScheduleEvent(self, 1.0f, nameof(DoExplode));
        }
        
        
        [GameplayEvent(typeof(GameplayOnHitEvent)), BurstCompile, Preserve, MonoPInvokeCallback(typeof(GameplayOnHitEvent.Delegate))]
        private static void OnHit(ref GameplayState state, ref Entity self, ref Entity target)
        {
            //state.DealDamage(self, 1.0f, target);
        }
        
        [GameplayEvent(typeof(GameplayScheduledEvent)), BurstCompile, Preserve, MonoPInvokeCallback(typeof(GameplayOnHitEvent.Delegate))]
        private static void DoExplode(ref GameplayState state, ref Entity self)
        {
            state.MarkForDestroy(self);
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