using System.Runtime.InteropServices;
using Battlemage.GameplayBehaviour.Data;
using Battlemage.GameplayBehaviour.Data.GameplayEvents;
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
            foreach (var (scheduledEvents, entity) in SystemAPI.Query<DynamicBuffer<GameplayScheduledEvent>>().WithEntityAccess())
            {
                for (var i = 0; i < scheduledEvents.Length; i++)
                {
                    var scheduledEvent = scheduledEvents[i];
                    if (scheduledEvent.Time <= 0)
                    {
                        var gameplayState = new GameplayState(ref state, ref ecb);
                        var source = entity;
                        Marshal.GetDelegateForFunctionPointer<GameplayScheduledEvent.Delegate>(scheduledEvent
                            .EventPointerRef.Value.Pointer).Invoke(ref gameplayState, ref source);
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