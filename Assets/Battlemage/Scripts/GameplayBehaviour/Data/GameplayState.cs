using Battlemage.Attributes.Data;
using Battlemage.GameplayBehaviour.Data.GameplayEvents;
using Battlemage.GameplayBehaviour.Utilities;
using BovineLabs.Core.Iterators;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;

namespace Battlemage.GameplayBehaviour.Data
{
    public ref struct GameplayState
    {
        private SystemState _state;
        private EntityCommandBuffer _ecb;

        public GameplayState(ref SystemState state, ref EntityCommandBuffer ecb)
        {
            _state = state;
            _ecb = ecb;
        }

        public void MarkForDestroy(Entity entity)
        {
            _ecb.DestroyEntity(entity);
        }

        public void SetVelocity(Entity entity, float3 velocity)
        {
            _state.EntityManager.SetComponentData(entity, new PhysicsVelocity()
            {
                Linear = velocity,
            });
        }

        public void DealDamage(Entity source, float amount, Entity target)
        {
            var attributeMap = _state.EntityManager.GetBuffer<AttributeMap>(target)
                .AsHashMap<AttributeMap, byte, AttributeValue>();
            var health = attributeMap[(byte)GameplayAttribute.Health];
            health.CurrentValue -= amount;
            attributeMap[(byte)GameplayAttribute.Health] = health;
        }

        public void ScheduleEvent(Entity entity, float time, GameplayScheduledEvent.Delegate scheduledEventDelegate)
        {
            _state.EntityManager.GetBuffer<GameplayScheduledEvent>(entity).Add(new GameplayScheduledEvent()
            {
                EventHash = GameplayBehaviourUtilities.GetEventHash(typeof(GameplayScheduledEvent), scheduledEventDelegate.Method),
                Time = time,
            });
        }

        public float3 GetForward(Entity entity)
        {
            return _state.EntityManager.GetComponentData<LocalToWorld>(entity).Forward;
        }
    }
}