using Battlemage.GameplayBehaviours.Data;
using Battlemage.GameplayBehaviours.Data.GameplayEvents;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using Waddle.GameplayBehaviour.Data;
using Waddle.GameplayBehaviour.Extensions;
using Waddle.GameplayBehaviour.Systems;

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
                for (var i = 0; i < scheduledEvents.Length; i++)
                {
                    var scheduledEvent = scheduledEvents[i];
                    if (SystemAPI.Time.ElapsedTime >= scheduledEvent.TimeStarted + scheduledEvent.Time)
                    {
                        var source = entity;
                        var pointer = eventMaps.GetEventPointer(eventPointers, scheduledEvent.TypeHash, scheduledEvent.MethodHash);
                        new FunctionPointer<GameplayScheduledEvent.Delegate>(pointer).Invoke(ref gameplayState, ref source);
                        scheduledEvents.RemoveAt(i);
                        i--;
                    }
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}