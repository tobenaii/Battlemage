using Unity.Entities;

namespace Battlemage.GameplayBehaviour.Data.GameplayEvents
{
    public struct GameplayOnHitEvent : IGameplayEvent
    {
        public delegate void Delegate(ref GameplayState gameplayState, ref Entity source, ref Entity target);
        public BlobAssetReference<EventPointer> EventPointerRef { get; set; }
    }
}