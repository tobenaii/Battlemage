using Unity.Entities;

namespace Battlemage.GameplayBehaviour.Data
{
    public interface IGameplayEvent : IComponentData
    {
        public BlobAssetReference<EventPointer> EventPointerRef { get; }
    }
}