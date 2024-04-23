using Battlemage.GameplayBehaviours.Data;
using Battlemage.GameplayBehaviours.Data.GameplayEvents;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using Waddle.GameplayBehaviours.Data;
using Waddle.GameplayBehaviours.Extensions;
using Waddle.GameplayBehaviours.Systems;

namespace Battlemage.GameplayBehaviours.Systems
{
    [BurstCompile]
    [UpdateInGroup(typeof(GameplayEventsSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    public partial struct GameplayScheduledEventSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameplayEventPointer>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            var eventPointers = SystemAPI.GetSingletonBuffer<GameplayEventPointer>();
            var gameplayState = new GameplayState(state.EntityManager, ecb, SystemAPI.Time, state.WorldUnmanaged.IsServer());
            
            foreach (var (scheduledEvents, eventMaps, entity) in SystemAPI
                         .Query<DynamicBuffer<GameplayScheduledEvent>, DynamicBuffer<GameplayEventReference>>()
                         .WithAll<Simulate>()
                         .WithEntityAccess())
            {
                foreach (var scheduledEvent in scheduledEvents)
                {
                    var timeToRun = scheduledEvent.TimeStarted + scheduledEvent.Time;
                    if (SystemAPI.Time.ElapsedTime >= timeToRun && SystemAPI.Time.ElapsedTime - SystemAPI.Time.DeltaTime < timeToRun)
                    {
                        var source = entity;
                        var pointer = eventMaps.GetEventPointer(eventPointers, scheduledEvent.TypeHash, scheduledEvent.MethodHash);
                        new FunctionPointer<GameplayScheduledEvent.Delegate>(pointer).Invoke(ref gameplayState, ref source);
                    }
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}