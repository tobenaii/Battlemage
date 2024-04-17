using BovineLabs.Core.Iterators;
using Unity.Entities;
using Waddle.Attributes.Data;

namespace Waddle.Attributes.Extensions
{
    public static class AttributeMapExtension
    {
        public static DynamicBuffer<AttributeMap> Initialize(this DynamicBuffer<AttributeMap> buffer)
        {
            return buffer.InitializeHashMap<AttributeMap, byte, AttributeValue>();
        }

        public static DynamicHashMap<byte, AttributeValue> AsMap(this DynamicBuffer<AttributeMap> buffer)
        {
            return buffer.AsHashMap<AttributeMap, byte, AttributeValue>();
        }
    }
}