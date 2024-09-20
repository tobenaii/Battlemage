using Unity.Entities;

namespace Waddle.GameplayBehaviours.Data
{
    public struct GameplayEventSetupData : IBufferElementData
    {
        public int GameplayBehaviourHash;
        public ulong EventHash;
        public BlobAssetReference<GameplayEventPointer> Pointer;
    }
}