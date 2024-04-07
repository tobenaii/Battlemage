using Unity.Entities;

namespace Battlemage.GameplayBehaviour.Data.GameplayEvents
{
    public struct GameplayOnSpawnEvent : IGameplayEvent
    {
        public delegate void Delegate(ref GameplayState gameplayState, ref Entity source);
        public BlobAssetReference<EventPointer> EventPointerRef { get; set; }
    }
}