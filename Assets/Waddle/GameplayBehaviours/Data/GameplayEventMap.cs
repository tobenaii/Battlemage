using Unity.Entities;

namespace Waddle.GameplayBehaviours.Data
{
    [InternalBufferCapacity(0)]
    public struct GameplayEventReference : IBufferElementData
    {
        public ulong TypeHash;
        public int MethodHash;
        public int Index;
    }
    
    public struct FullGameplayEventReference : IBufferElementData
    {
        public ulong TypeHash;
        public int MethodHash;
        public Hash128 FullHash;
    }
}