using BovineLabs.Core.Iterators;
using Unity.Entities;
using Waddle.GameplayAttributes.Data;

namespace Waddle.GameplayAttributes.Extensions
{
    public static class GameplayAttributeMapExtension
    {
        public static DynamicBuffer<GameplayAttributeMap> Initialize(this DynamicBuffer<GameplayAttributeMap> buffer)
        {
            return buffer.InitializeHashMap<GameplayAttributeMap, byte, GameplayAttribute>();
        }

        public static DynamicHashMap<byte, GameplayAttribute> AsMap(this DynamicBuffer<GameplayAttributeMap> buffer)
        {
            return buffer.AsHashMap<GameplayAttributeMap, byte, GameplayAttribute>();
        }
    }
}