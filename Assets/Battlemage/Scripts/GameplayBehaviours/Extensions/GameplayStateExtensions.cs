using Battlemage.GameplayBehaviours.Data.GameplayEvents;
using Unity.Collections;
using Unity.Entities;
using Waddle.GameplayBehaviours.Data;
using Waddle.GameplayBehaviours.Extensions;

namespace Battlemage.GameplayBehaviours.Extensions
{
    public static class GameplayStateExtensions
    {
        public static void ScheduleEvent(this GameplayState gameplayState, Entity entity, float time, FixedString64Bytes scheduledEvent)
        {
            var eventReferences = gameplayState.GetBuffer<GameplayEventReference>(entity);
            var eventHash = new GameplayEventHash()
            {
                TypeHash = TypeManager.GetTypeInfo<GameplayScheduledEvent>().StableTypeHash,
                MethodHash = scheduledEvent.GetHashCode(),
            };
            var eventIndex = eventReferences.GetEventIndex(eventHash.TypeHash, eventHash.MethodHash);
            gameplayState.GetBuffer<GameplayScheduledEvent>(entity).Add(new GameplayScheduledEvent()
            {
                EventIndex = eventIndex,
                Countdown = time,
            });
        }
        
        public static void AddOverlapSphereCallback(this GameplayState gameplayState, Entity entity, float radius, FixedString64Bytes scheduledEvent)
        {
            var eventReferences = gameplayState.GetBuffer<GameplayEventReference>(entity);
            var eventHash = new GameplayEventHash()
            {
                TypeHash = TypeManager.GetTypeInfo<GameplayOnHitEvent>().StableTypeHash,
                MethodHash = scheduledEvent.GetHashCode(),
            };
            var eventIndex = eventReferences.GetEventIndex(eventHash.TypeHash, eventHash.MethodHash);
            gameplayState.GetBuffer<GameplayOnHitEvent>(entity).Add(new GameplayOnHitEvent()
            {
                EventIndex = eventIndex,
                Radius = radius
            });
        }
    }
}