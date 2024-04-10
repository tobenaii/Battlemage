using Unity.Entities;

namespace Waddle.GameplayBehaviour.Data
{
    public struct GameplayEventBlobMapping : IComponentData
    {
        public Hash128 Hash;
        public BlobAssetReference<EventPointer> Pointer;
    }
}