using BovineLabs.Core.Iterators;
using Unity.Entities;
using Unity.NetCode;

namespace Waddle.Attributes.Data
{
    [InternalBufferCapacity(0)]
    public struct AttributeMap : IDynamicHashMap<byte, AttributeValue>
    {
        [GhostField] public byte Data;
        
        public byte Value => Data;
    }

    public struct AttributeValue
    {
        [GhostField] public float BaseValue;
        [GhostField] public float CurrentValue;
    }
}