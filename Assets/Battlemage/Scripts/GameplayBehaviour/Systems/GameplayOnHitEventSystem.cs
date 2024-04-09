﻿using System;
using System.Runtime.InteropServices;
using Battlemage.GameplayBehaviour.Data;
using Battlemage.GameplayBehaviour.Data.GameplayEvents;
using Battlemage.GameplayBehaviour.Extensions;
using Battlemage.GameplayBehaviour.Utilities;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using Hash128 = Unity.Entities.Hash128;

namespace Battlemage.GameplayBehaviour.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct GameplayOnHitEventSystem : ISystem
    {
        private static readonly Hash128 EventHash = GameplayBehaviourUtilities.GetEventHash(typeof(GameplayOnHitEvent));
        
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
                    var gameplayState = new GameplayState(ref state, ref ecb);
                    var pointer = eventRefs.GetEventPointer(EventHash);
                    Marshal.GetDelegateForFunctionPointer<GameplayOnHitEvent.Delegate>(pointer).Invoke(ref gameplayState, ref ability, ref target);
                }
            }
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}