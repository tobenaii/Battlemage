using AOT;
using Battlemage.GameplayBehaviours.Data.GameplayEvents;
using Battlemage.GameplayBehaviours.Extensions;
using Battlemage.SimpleVelocity.Data;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine.Scripting;
using Waddle.FirstPersonCharacter.Data;
using Waddle.GameplayAbilities.Authoring;
using Waddle.GameplayAbilities.Data;
using Waddle.GameplayBehaviours.Data;

namespace Battlemage.Abilities.Authoring
{
    [BurstCompile, Preserve]
    public class FireballAbilityAuthoring : AbilityBehaviour
    {
        protected override void Bake(Entity entity)
        {
            base.Bake(entity);
            Baker.AddComponent(entity, new Velocity());
        }
        
        [GameplayEvent(typeof(GameplayOnSpawnEvent)), BurstCompile, Preserve, MonoPInvokeCallback(typeof(GameplayOnSpawnEvent.Delegate))]
        private static void OnSpawn(ref GameplayState state, ref Entity self)
        {
            var abilityData = state.GetComponent<GameplayAbilityData>(self);
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
            var abilityData = state.GetComponent<GameplayAbilityData>(self);
            if (target == abilityData.Source) return;
            DoExplode(ref state, ref self);
        }
        
        [GameplayEvent(typeof(GameplayScheduledEvent)), BurstCompile, Preserve, MonoPInvokeCallback(typeof(GameplayOnHitEvent.Delegate))]
        private static void DoExplode(ref GameplayState state, ref Entity self)
        {
            state.MarkForDestroy(self);
        }
    }
}