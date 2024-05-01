using System;
using Battlemage.GameplayBehaviours.Data.GameplayEvents;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Waddle.GameplayBehaviours.Data;
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
            var deltaTime = SystemAPI.Time.DeltaTime;
            
            foreach (var (scheduledEvents, entity) in SystemAPI
                         .Query<DynamicBuffer<GameplayScheduledEvent>>()
                         .WithAll<Simulate>()
                         .WithEntityAccess())
            {
                for (var i = 0; i < scheduledEvents.Length; i++)
                {
                    var scheduledEvent = scheduledEvents[i];
                    if (scheduledEvent.Countdown > 0 && scheduledEvent.Countdown - deltaTime <= 0)
                    {
                        var source = entity;
                        var pointer = new IntPtr(eventPointers[scheduledEvent.EventIndex].Pointer);
                        new FunctionPointer<GameplayScheduledEvent.Delegate>(pointer).Invoke(ref gameplayState,
                            ref source);
                        scheduledEvents.RemoveAt(i);
                        i--;
                        continue;
                    }

                    scheduledEvent.Countdown -= deltaTime;
                    scheduledEvents[i] = scheduledEvent;
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}