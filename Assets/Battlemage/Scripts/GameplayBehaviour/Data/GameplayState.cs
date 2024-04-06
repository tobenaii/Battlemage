using Battlemage.Attributes.Data;
using BovineLabs.Core.Iterators;
using Unity.Entities;

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
        
        public void DealDamage(float amount, Entity target)
        {
            var attributeMap = _state.EntityManager.GetBuffer<AttributeMap>(target).AsHashMap<AttributeMap, byte, AttributeValue>();
            var health = attributeMap[(byte)GameplayAttribute.Health];
            health.CurrentValue -= amount;
            attributeMap[(byte)GameplayAttribute.Health] = health;
        }
    }
}