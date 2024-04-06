using System;
using System.Runtime.InteropServices;
using Battlemage.GameplayBehaviour.Data;
using Unity.Collections;
using Unity.Entities;
using Unity.Physics;
using Unity.Transforms;

namespace Battlemage.GameplayBehaviour.Systems
{
    public partial struct GameplayOnHitCallbackSystem : ISystem
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
            
            foreach (var (onHit, localTransform, entity) in
                     SystemAPI.Query<RefRO<GameplayOnHitCallback>, LocalTransform>()
                         .WithEntityAccess())
            {
                _hits.Clear();
                if (collisionWorld.OverlapSphere(localTransform.Position, 1, ref _hits, CollisionFilter.Default))
                {
                    var ability = entity;
                    var target = _hits[0].Entity;
                    Marshal.GetDelegateForFunctionPointer<GameplayOnHitCallback.Delegate>(new IntPtr(onHit.ValueRO.Callback)).Invoke(ref state, ref ability, ref target);
                }
            }
        }
    }
}