using Battlemage.GameplayBehaviours.Data;
using Battlemage.GameplayBehaviours.Data.GameplayEvents;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using Waddle.GameplayBehaviour.Data;
using Waddle.GameplayBehaviour.Extensions;

namespace Battlemage.GameplayBehaviours.Systems
{
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct GameplayOnHitEventSystem : ISystem
    {
        private NativeList<DistanceHit> _hits;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PhysicsWorldSingleton>();
            state.RequireForUpdate<GameplayEventPointer>();
            _hits = new NativeList<DistanceHit>(Allocator.Persistent);
        }
        
        [BurstCompile]
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
            var eventPointers = SystemAPI.GetSingletonBuffer<GameplayEventPointer>();
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (eventRefs, localTransform, entity) in
                     SystemAPI.Query<DynamicBuffer<GameplayEventReference>, LocalTransform>()
                         .WithAll<GameplayOnHitEvent>()
                         .WithEntityAccess())
            {
                _hits.Clear();
                if (collisionWorld.OverlapSphere(localTransform.Position, 1, ref _hits, CollisionFilter.Default))
                {
                    var ability = entity;
                    var target = _hits[0].Entity;
                    var gameplayState = new GameplayState(state.EntityManager, ref ecb);
                    var pointer = eventRefs.GetEventPointer(eventPointers, TypeManager.GetTypeInfo<GameplayOnHitEvent>().StableTypeHash);
                    new FunctionPointer<GameplayOnHitEvent.Delegate>(pointer).Invoke(ref gameplayState, ref ability, ref target);
                }
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}