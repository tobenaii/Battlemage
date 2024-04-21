using AOT;
using Battlemage.GameplayBehaviours.Data;
using Battlemage.GameplayBehaviours.Data.GameplayEvents;
using Battlemage.SimpleVelocity.Data;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Scripting;
using Waddle.Abilities.Data;
using Waddle.FirstPersonCharacter.Data;
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
            var viewEntity = state.GetComponent<CharacterSettings>(abilityData.Source).ViewEntity;
            var viewTransform = state.GetComponent<LocalTransform>(viewEntity);
            
            playerTransform.Position += playerTransform.Up() * 1.25f;
            playerTransform.Rotation = math.mul(playerTransform.Rotation, viewTransform.Rotation);
            playerTransform.Position += playerTransform.Forward();
            state.SetComponent(self, playerTransform);

            var velocity = new Velocity() { Value = playerTransform.Forward() * 20.0f };
            state.SetComponent(self, velocity);
            
            state.ScheduleEvent(self, 10.0f, nameof(DoExplode));
            state.AddOverlapSphereCallback(self, 0.25f, nameof(OnHit));
        }
        
        [GameplayEvent(typeof(GameplayOnHitEvent)), BurstCompile, Preserve, MonoPInvokeCallback(typeof(GameplayOnHitEvent.Delegate))]
        private static void OnHit(ref GameplayState state, ref Entity self, ref Entity target)
        {
            var abilityData = state.GetComponent<AbilityData>(self);
            if (target == abilityData.Source) return;
            DoExplode(ref state, ref self);
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
                AddComponent(entity, new Velocity());
            }
        }
    }
}