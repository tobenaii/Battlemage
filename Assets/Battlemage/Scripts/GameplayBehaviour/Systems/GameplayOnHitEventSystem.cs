using System;
using System.Runtime.InteropServices;
using Battlemage.GameplayBehaviour.Data;
using Battlemage.GameplayBehaviour.Data.GameplayEvents;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;

namespace Battlemage.GameplayBehaviour.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
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

        public void OnUpdate(ref SystemState state)
        {
            var collisionWorld = SystemAPI.GetSingleton<PhysicsWorldSingleton>().CollisionWorld;

            var ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach (var (onHit, localTransform, entity) in
                     SystemAPI.Query<RefRO<GameplayOnHitEvent>, LocalTransform>()
                         .WithEntityAccess())
            {
                _hits.Clear();
                if (collisionWorld.OverlapSphere(localTransform.Position, 1, ref _hits, CollisionFilter.Default))
                {
                    var ability = entity;
                    var target = _hits[0].Entity;
                    var gameplayState = new GameplayState(ref state, ref ecb);
                    Marshal.GetDelegateForFunctionPointer<GameplayOnHitEvent.Delegate>(onHit.ValueRO.EventPointerRef.Value.Pointer).Invoke(ref gameplayState, ref ability, ref target);
                }
            }
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}