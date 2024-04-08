using Unity.Entities;

namespace Battlemage.GameplayBehaviour.Data
{
    public interface IGameplayEvent : IComponentData
    {
        public BlobAssetReference<EventPointer> EventPointerRef { get; set; }
    }
    
    public interface IGameplayEventBuffer : IBufferElementData
    {
        public BlobAssetReference<EventPointer> EventPointerRef { get; set; }
    }
}