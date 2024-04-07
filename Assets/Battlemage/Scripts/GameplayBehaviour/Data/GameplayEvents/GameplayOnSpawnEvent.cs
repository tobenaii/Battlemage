using Unity.Entities;

namespace Battlemage.GameplayBehaviour.Data.GameplayEvents
{
    [GameplayEventDefinition(typeof(Delegate))]
    public struct GameplayOnSpawnEvent : IGameplayEvent
    {
        public delegate void Delegate(ref GameplayState gameplayState, ref Entity self);
        public BlobAssetReference<EventPointer> EventPointerRef { get; set; }
    }
}