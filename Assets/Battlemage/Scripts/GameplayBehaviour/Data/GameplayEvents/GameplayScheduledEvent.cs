using Unity.Entities;

namespace Battlemage.GameplayBehaviour.Data.GameplayEvents
{
    [GameplayEventDefinition(typeof(Delegate))]
    public struct GameplayScheduledEvent : IGameplayEvent
    {
        public delegate void Delegate(ref GameplayState gameplayState, ref Entity source);
        public BlobAssetReference<EventPointer> EventPointerRef { get; set; }
    }
}