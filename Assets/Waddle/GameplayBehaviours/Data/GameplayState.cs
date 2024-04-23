using Unity.Core;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Waddle.GameplayAbilities.Data;
using Waddle.GameplayActions.Data;
using Waddle.Utilities;

namespace Waddle.GameplayBehaviours.Data
{
    public ref struct GameplayState
    {
        private EntityManager _entityManager;
        private EntityCommandBuffer _ecb;
        private readonly TimeData _time;
        private readonly BlittableBool _isServer;

        public TimeData Time => _time;
        
        public GameplayState(EntityManager entityManager, EntityCommandBuffer ecb, TimeData time, BlittableBool isServer)
        {
            _entityManager = entityManager;
            _ecb = ecb;
            _time = time;
            _isServer = isServer;
        }

        public T GetComponent<T>(Entity entity) where T : unmanaged, IComponentData
        {
            return _entityManager.GetComponentData<T>(entity);
        }
        
        public void SetComponent<T>(Entity entity, T component) where T : unmanaged, IComponentData
        {
            _entityManager.SetComponentData(entity, component);
        }

        public DynamicBuffer<T> GetBuffer<T>(Entity entity) where T : unmanaged, IBufferElementData
        {
            return _entityManager.GetBuffer<T>(entity);
        }
        
        public void MarkForDestroy(Entity entity)
        {
            if (_isServer)
                _ecb.DestroyEntity(entity);
            else
                _ecb.SetEnabled(entity, false);
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