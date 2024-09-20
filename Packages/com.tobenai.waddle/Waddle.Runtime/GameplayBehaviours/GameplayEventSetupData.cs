using Unity.Entities;

namespace Waddle.Runtime.GameplayBehaviours
{
    public struct GameplayEventSetupData : IBufferElementData
    {
        public int GameplayBehaviourHash;
        public ulong EventHash;
        public BlobAssetReference<GameplayEventPointer> Pointer;
    }
}