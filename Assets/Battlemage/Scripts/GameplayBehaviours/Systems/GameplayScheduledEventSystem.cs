using Battlemage.GameplayBehaviours.Data;
using Battlemage.GameplayBehaviours.Data.GameplayEvents;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Waddle.GameplayBehaviour.Data;
using Waddle.GameplayBehaviour.Extensions;

namespace Battlemage.GameplayBehaviours.Systems
{
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
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

            foreach (var (scheduledEvents, eventMaps, entity) in SystemAPI
                         .Query<DynamicBuffer<GameplayScheduledEvent>, DynamicBuffer<GameplayEventReference>>()
                         .WithEntityAccess())
            {
                for (var i = 0; i < scheduledEvents.Length; i++)
                {
                    var scheduledEvent = scheduledEvents[i];
                    if (scheduledEvent.Time <= 0)
                    {
                        var gameplayState = new GameplayState(state.EntityManager, ref ecb);
                        var source = entity;
                        var pointer = eventMaps.GetEventPointer(eventPointers, scheduledEvent.TypeHash, scheduledEvent.MethodHash);
                        new FunctionPointer<GameplayScheduledEvent.Delegate>(pointer).Invoke(ref gameplayState, ref source);
                        scheduledEvents.RemoveAt(i);
                        i--;
                    }
                    else
                    {
                        scheduledEvent.Time -= SystemAPI.Time.DeltaTime;
                        scheduledEvents[i] = scheduledEvent;
                    }
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}