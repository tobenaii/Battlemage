using Unity.Entities;

namespace Battlemage.GameplayBehaviour.Data.GameplayEvents
{
    [GameplayEventDefinition]
    public struct GameplayScheduledEvent : IGameplayEvent
    {
        public BlobAssetReference<EventPointer> EventPointerRef { get; set; }
    }
}