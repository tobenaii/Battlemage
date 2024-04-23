using BovineLabs.Core.Iterators;
using Unity.Entities;
using Unity.NetCode;

namespace Waddle.GameplayAttributes.Data
{
    [InternalBufferCapacity(0)]
    public struct GameplayAttributeMap : IDynamicHashMap<byte, GameplayAttribute>
    {
        [GhostField] public byte Data;
        
        public byte Value => Data;
    }
}