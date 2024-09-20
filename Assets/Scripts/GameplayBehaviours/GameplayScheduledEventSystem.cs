using Battlemage.GameplayBehaviours.GameplayEvents;
using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using Waddle.Runtime.GameplayBehaviours;
using Waddle.Runtime.GameplayLifecycle;

namespace Battlemage.GameplayBehaviours
{
    [UpdateInGroup(typeof(GameplayEventsSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [BurstCompile]
    public partial struct GameplayScheduledEventSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var instantiateEcb = SystemAPI.GetSingletonRW<InstantiateCommandBufferSystem.Singleton>().ValueRW
                .CreateCommandBuffer(state.WorldUnmanaged);
            var gameplayState = new GameplayState(state.EntityManager, instantiateEcb, SystemAPI.Time, state.WorldUnmanaged.IsServer());
            
            foreach (var (eventRefs, scheduledEventsBuffer, entity) in SystemAPI
                         .Query<DynamicBuffer<GameplayEventReference>, DynamicBuffer<GameplayScheduledEvent>>()
                         .WithEntityAccess())
            {
                var scheduledEvents = scheduledEventsBuffer;
                for (var i = 0; i < scheduledEvents.Length; i++)
                {
                    var scheduledEvent = scheduledEvents[i];
                    scheduledEvent.Seconds -= SystemAPI.Time.DeltaTime;
                    scheduledEvents[i] = scheduledEvent;
                    if (scheduledEvent.Seconds <= 0)
                    {
                        var source = entity;
                        new FunctionPointer<GameplayScheduledEvent.Delegate>(eventRefs[scheduledEvent.EventBlobIndex].Pointer.Value.Pointer).Invoke(ref gameplayState, ref source);
                        scheduledEvents.RemoveAt(i);
                        i--;
                    }
                }
            }
        }
    }
}