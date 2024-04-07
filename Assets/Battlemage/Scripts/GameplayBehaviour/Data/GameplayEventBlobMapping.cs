using Unity.Entities;

namespace Battlemage.GameplayBehaviour.Data
{
    public struct GameplayEventBlobMapping : IComponentData
    {
        public Hash128 Hash;
        public BlobAssetReference<EventPointer> Pointer;
    }
}