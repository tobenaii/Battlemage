using System;
using Unity.Core;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Waddle.EntitiesExtended;
using Waddle.EntitiesExtended.Extensions;
using Waddle.GameplayLifecycle.Data;

namespace Waddle.GameplayBehaviours.Data
{
    public struct GameplayState
    {
        private EntityManager _entityManager;
        private EntityCommandBuffer _instantiateEcb;
        private readonly TimeData _time;
        private readonly BlittableBool _isServer;

        public TimeData Time => _time;
        public bool IsServer => _isServer;
        
        public GameplayState(EntityManager entityManager, EntityCommandBuffer instantiateEcb, TimeData time, BlittableBool isServer)
        {
            _entityManager = entityManager;
            _instantiateEcb = instantiateEcb;
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
        
        public void SetComponent<T>(TemporaryEntity temporaryEntity, T component) where T : unmanaged, IComponentData
        {
            _instantiateEcb.SetComponent(temporaryEntity.Entity, component);
        }

        public T GetSingleton<T>() where T : unmanaged, IComponentData
        {
            return _entityManager.GetSingleton<T>();
        }

        public T GetSingletonManaged<T>() where T : class, IComponentData, new()
        {
            return _entityManager.GetSingletonManaged<T>();
        }
        
        public DynamicBuffer<T> GetSingletonBuffer<T>() where T : unmanaged, IBufferElementData
        {
            return _entityManager.GetSingletonBuffer<T>();
        }

        public DynamicBuffer<T> GetBuffer<T>(Entity entity) where T : unmanaged, IBufferElementData
        {
            return _entityManager.GetBuffer<T>(entity);
        }
        
        public DynamicBuffer<T> GetBuffer<T>(TemporaryEntity temporaryEntity) where T : unmanaged, IBufferElementData
        {
            return _instantiateEcb.SetBuffer<T>(temporaryEntity.Entity);
        }

        public TemporaryEntity Instantiate(Entity prefab)
        {
            return new TemporaryEntity()
            {
                Entity = _instantiateEcb.Instantiate(prefab)
            };
        }
        
        public void Destroy(Entity entity)
        {
            _entityManager.SetComponentEnabled<DestroyEntity>(entity, true);
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