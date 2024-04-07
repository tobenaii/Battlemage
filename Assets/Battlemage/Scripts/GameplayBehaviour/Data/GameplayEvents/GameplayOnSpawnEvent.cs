using Unity.Entities;

namespace Battlemage.GameplayBehaviour.Data.GameplayEvents
{
    [GameplayEventDefinition]
    public struct GameplayOnSpawnEvent : IGameplayEvent
    {
        public BlobAssetReference<EventPointer> EventPointerRef { get; set; }
    }
}