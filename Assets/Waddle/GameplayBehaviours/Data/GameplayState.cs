using Unity.Core;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Waddle.EntitiesExtended;
using Waddle.EntitiesExtended.Extensions;

namespace Waddle.GameplayBehaviours.Data
{
    public struct GameplayState
    {
        private EntityManager _entityManager;
        private EntityCommandBuffer _ecb;
        private readonly TimeData _time;
        private readonly BlittableBool _isServer;

        public TimeData Time => _time;
        public bool IsServer => _isServer;
        
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

        public T GetComponentManaged<T>(Entity entity) where T : class, IComponentData, new()
        {
            return _entityManager.GetComponentData<T>(entity);
        }
        
        public void SetComponent<T>(Entity entity, T component) where T : unmanaged, IComponentData
        {
            _entityManager.SetComponentData(entity, component);
        }

        public T GetSingleton<T>() where T : unmanaged, IComponentData
        {
            return _entityManager.GetSingleton<T>();
        }
        
        public DynamicBuffer<T> GetSingletonBuffer<T>() where T : unmanaged, IBufferElementData
        {
            return _entityManager.GetSingletonBuffer<T>();
        }

        public DynamicBuffer<T> GetBuffer<T>(Entity entity) where T : unmanaged, IBufferElementData
        {
            return _entityManager.GetBuffer<T>(entity);
        }

        public Entity Instantiate(Entity prefab)
        {
            return _entityManager.Instantiate(prefab);
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