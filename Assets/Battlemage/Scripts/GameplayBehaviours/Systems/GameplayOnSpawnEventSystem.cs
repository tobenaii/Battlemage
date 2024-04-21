using Battlemage.GameplayBehaviours.Data;
using Battlemage.GameplayBehaviours.Data.GameplayEvents;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using Waddle.GameplayBehaviour.Data;
using Waddle.GameplayBehaviour.Extensions;

namespace Battlemage.GameplayBehaviours.Systems
{
    [BurstCompile]
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup), OrderLast = true)]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    public partial struct GameplayOnSpawnEventSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameplayEventPointer>();
            state.RequireForUpdate<NetworkTime>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            var eventPointers = SystemAPI.GetSingletonBuffer<GameplayEventPointer>();
            var networkTime = SystemAPI.GetSingleton<NetworkTime>();
            var gameplayState = new GameplayState(state.EntityManager, ecb, SystemAPI.Time, state.WorldUnmanaged.IsServer());

            if (!networkTime.IsFirstTimeFullyPredictingTick) return;

            foreach (var (eventRefs, entity) in SystemAPI.Query<DynamicBuffer<GameplayEventReference>>()
                         .WithAll<GameplayOnSpawnEvent>()
                         .WithEntityAccess())
            {
                var source = entity;
                var pointer = eventRefs.GetEventPointer(eventPointers, TypeManager.GetTypeInfo<GameplayOnSpawnEvent>().StableTypeHash);
                new FunctionPointer<GameplayOnSpawnEvent.Delegate>(pointer).Invoke(ref gameplayState, ref source);
                ecb.RemoveComponent<GameplayOnSpawnEvent>(entity);
            }
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}