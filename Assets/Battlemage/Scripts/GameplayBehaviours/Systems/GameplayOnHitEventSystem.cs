using Battlemage.GameplayBehaviours.Data.GameplayEvents;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using Waddle.GameplayBehaviours.Data;
using Waddle.GameplayBehaviours.Systems;
using Waddle.GameplayLifecycle.Systems;

namespace Battlemage.GameplayBehaviours.Systems
{
    [UpdateInGroup(typeof(GameplayEventsSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [BurstCompile]
    public partial struct GameplayOnHitEventSystem : ISystem
    {
        private NativeList<DistanceHit> _hits;
        
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PhysicsWorldSingleton>();
            _hits = new NativeList<DistanceHit>(Allocator.Persistent);
        }
        
        public void OnDestroy(ref SystemState state)
        {
            if (_hits.IsCreated)
            {
                _hits.Dispose();
            }
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;
            var instantiateEcb = SystemAPI.GetSingletonRW<InstantiateCommandBufferSystem.Singleton>().ValueRW
                .CreateCommandBuffer(state.WorldUnmanaged);            
            var gameplayState = new GameplayState(state.EntityManager, instantiateEcb, SystemAPI.Time, state.WorldUnmanaged.IsServer());
            foreach (var (onHitEvents, localTransform, entity) in
                     SystemAPI.Query<DynamicBuffer<GameplayOnHitEvent>, LocalTransform>()
                         .WithAll<Simulate>()
                         .WithEntityAccess())
            {
                _hits.Clear();
                foreach (var onHitEvent in onHitEvents)
                {
                    if (collisionWorld.OverlapSphere(localTransform.Position, onHitEvent.Radius, ref _hits,
                            CollisionFilter.Default))
                    {
                        var hit = _hits[0];
                        var self = entity;
                        var target = hit.Entity;
                        new FunctionPointer<GameplayOnHitEvent.Delegate>(onHitEvent.EventBlob.Value.Pointer).Invoke(ref gameplayState, ref self, ref target);
                    }
                }
            }
        }
    }
}