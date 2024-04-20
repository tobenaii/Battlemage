using Battlemage.Attributes.Data;
using Battlemage.GameplayBehaviours.Data.GameplayEvents;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Waddle.Abilities.Data;
using Waddle.Attributes.Data;
using Waddle.Attributes.Extensions;

namespace Battlemage.GameplayBehaviours.Data
{
    public ref struct GameplayState
    {
        private EntityManager _entityManager;
        private EntityCommandBuffer _ecb;
        
        public GameplayState(EntityManager entityManager, ref EntityCommandBuffer ecb)
        {
            _entityManager = entityManager;
            _ecb = ecb;
        }

        public T GetComponent<T>(Entity entity) where T : unmanaged, IComponentData
        {
            return _entityManager.GetComponentData<T>(entity);
        }
        
        public void SetComponent<T>(Entity entity, T component) where T : unmanaged, IComponentData
        {
            _entityManager.SetComponentData(entity, component);
        }

        public void TryActivateAbility(Entity entity, Entity abilityPrefab)
        {
            var requests = _entityManager.GetBuffer<AbilityActivateRequest>(entity);
            requests.Add(new AbilityActivateRequest()
            {
                AbilityPrefab = abilityPrefab
            });
        }
        
        public void MarkForDestroy(Entity entity)
        {
            _ecb.DestroyEntity(entity);
        }

        public void SetVelocity(Entity entity, float3 velocity)
        {
            _entityManager.SetComponentData(entity, new Velocity.Data.Velocity()
            {
                Value = velocity,
            });
        }

        public void DealDamage(Entity source, float amount, Entity target)
        {
            var attributeMap = _entityManager.GetBuffer<AttributeMap>(target).AsMap();
            var health = attributeMap[(byte)BattlemageAttribute.Health];
            health.CurrentValue -= amount;
            attributeMap[(byte)BattlemageAttribute.Health] = health;
        }

        public void ScheduleEvent(Entity entity, float time, FixedString64Bytes scheduledEvent)
        {
            _entityManager.GetBuffer<GameplayScheduledEvent>(entity).Add(new GameplayScheduledEvent()
            {
                TypeHash = TypeManager.GetTypeInfo<GameplayScheduledEvent>().StableTypeHash,
                MethodHash = scheduledEvent.GetHashCode(),
                Time = time,
            });
        }

        public float3 GetForward(Entity entity)
        {
            return _entityManager.GetComponentData<LocalTransform>(entity).Forward();
        }
        
        public float3 GetRight(Entity entity)
        {
            return _entityManager.GetComponentData<LocalTransform>(entity).Right();
        }
    }
}