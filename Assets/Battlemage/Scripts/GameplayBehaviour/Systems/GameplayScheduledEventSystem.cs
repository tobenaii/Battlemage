using System.Runtime.InteropServices;
using Battlemage.GameplayBehaviour.Data;
using Battlemage.GameplayBehaviour.Data.GameplayEvents;
using Battlemage.GameplayBehaviour.Extensions;
using Unity.Collections;
using Unity.Entities;

namespace Battlemage.GameplayBehaviour.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct GameplayScheduledEventSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach (var (scheduledEvents, eventMaps, entity) in SystemAPI
                         .Query<DynamicBuffer<GameplayScheduledEvent>, DynamicBuffer<GameplayEventReference>>()
                         .WithEntityAccess())
            {
                for (var i = 0; i < scheduledEvents.Length; i++)
                {
                    var scheduledEvent = scheduledEvents[i];
                    if (scheduledEvent.Time <= 0)
                    {
                        var gameplayState = new GameplayState(ref state, ref ecb);
                        var source = entity;
                        var pointer = eventMaps.GetEventPointer(scheduledEvent.EventHash);
                        Marshal.GetDelegateForFunctionPointer<GameplayScheduledEvent.Delegate>(pointer)
                            .Invoke(ref gameplayState, ref source);
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