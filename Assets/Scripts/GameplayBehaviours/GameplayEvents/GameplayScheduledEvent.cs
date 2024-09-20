using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Waddle.Runtime.GameplayBehaviours;

namespace Battlemage.GameplayBehaviours.GameplayEvents
{
    [GameplayEventDefinition(typeof(Delegate))]
    [GhostComponent]
    public struct GameplayScheduledEvent : IBufferElementData
    {
        public delegate void Delegate(ref GameplayState gameplayState, ref Entity source);
        [GhostField]
        public int EventBlobIndex;
        [GhostField]
        public float Seconds;
        
        public static void Schedule(GameplayState gameplayState, Entity source, float seconds, FixedString32Bytes methodName)
        {
            var buffer = gameplayState.GetBuffer<GameplayScheduledEvent>(source);
            var gameplayEventRefs = gameplayState.GetBuffer<GameplayEventReference>(source);
            var eventBlobIndex = gameplayEventRefs.GetEventPointerBlobIndex(TypeManager.GetTypeInfo<GameplayScheduledEvent>().StableTypeHash, methodName);
            buffer.Add(new GameplayScheduledEvent
            {
                EventBlobIndex = eventBlobIndex,
                Seconds = seconds
            });
        }
    }
}