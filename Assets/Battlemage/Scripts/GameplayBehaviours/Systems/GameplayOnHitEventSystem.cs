using System;
using Battlemage.GameplayBehaviours.Data.GameplayEvents;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using Waddle.GameplayBehaviours.Data;
using Waddle.GameplayBehaviours.Systems;

namespace Battlemage.GameplayBehaviours.Systems
{
    [BurstCompile]
    [UpdateInGroup(typeof(GameplayEventsSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
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
            var gameplayState = new GameplayState(state.EntityManager, ecb, SystemAPI.Time, state.WorldUnmanaged.IsServer());
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
                        var pointer = new IntPtr(eventPointers[onHitEvent.EventIndex].Pointer);
                        new FunctionPointer<GameplayOnHitEvent.Delegate>(pointer).Invoke(ref gameplayState, ref self,
                            ref target);
                    }
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}