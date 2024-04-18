using Battlemage.Attributes.Data;
using Battlemage.GameplayBehaviours.Data.GameplayEvents;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Waddle.Abilities.Data;
using Waddle.Attributes.Data;
using Waddle.Attributes.Extensions;
using Waddle.GameplayBehaviour.Utilities;

namespace Battlemage.GameplayBehaviours.Data
{
    [BurstCompile]
    public ref struct GameplayState
    {
        private EntityManager _entityManager;
        private EntityCommandBuffer _ecb;
        
        public GameplayState(EntityManager entityManager, ref EntityCommandBuffer ecb)
        {
            _entityManager = entityManager;
            _ecb = ecb;
        }

        [BurstCompile]
        public T GetComponent<T>(Entity entity) where T : unmanaged, IComponentData
        {
            return _entityManager.GetComponentData<T>(entity);
        }
        
        [BurstCompile]
        public void SetComponent<T>(Entity entity, T component) where T : unmanaged, IComponentData
        {
            _entityManager.SetComponentData(entity, component);
        }

        [BurstCompile]
        public void TryActivateAbility(Entity entity, Entity abilityPrefab)
        {
            var requests = _entityManager.GetBuffer<AbilityActivateRequest>(entity);
            requests.Add(new AbilityActivateRequest()
            {
                AbilityPrefab = abilityPrefab
            });
        }
        
        [BurstCompile]
        public void MarkForDestroy(Entity entity)
        {
            _ecb.DestroyEntity(entity);
        }

        [BurstCompile]
        public void SetVelocity(Entity entity, float3 velocity)
        {
            _entityManager.SetComponentData(entity, new Velocity.Data.Velocity()
            {
                Value = velocity,
            });
        }

        [BurstCompile]
        public void DealDamage(Entity source, float amount, Entity target)
        {
            var attributeMap = _entityManager.GetBuffer<AttributeMap>(target).AsMap();
            var health = attributeMap[(byte)BattlemageAttribute.Health];
            health.CurrentValue -= amount;
            attributeMap[(byte)BattlemageAttribute.Health] = health;
        }

        [BurstCompile]
        public void ScheduleEvent(Entity entity, float time, GameplayScheduledEvent.Delegate scheduledEventDelegate)
        {
            _entityManager.GetBuffer<GameplayScheduledEvent>(entity).Add(new GameplayScheduledEvent()
            {
                EventHash = GameplayBehaviourUtilities.GetEventHash(typeof(GameplayScheduledEvent), scheduledEventDelegate.Method),
                Time = time,
            });
        }

        [BurstCompile]
        public float3 GetForward(Entity entity)
        {
            return _entityManager.GetComponentData<LocalTransform>(entity).Forward();
        }
        
        [BurstCompile]
        public float3 GetRight(Entity entity)
        {
            return _entityManager.GetComponentData<LocalTransform>(entity).Right();
        }
    }
}