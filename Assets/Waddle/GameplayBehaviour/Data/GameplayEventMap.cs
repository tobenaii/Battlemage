using Unity.Entities;

namespace Waddle.GameplayBehaviour.Data
{
    [InternalBufferCapacity(0)]
    public struct GameplayEventReference : IBufferElementData
    {
        public Hash128 Hash;
        public BlobAssetReference<EventPointer> EventPointerRef;
    }
}