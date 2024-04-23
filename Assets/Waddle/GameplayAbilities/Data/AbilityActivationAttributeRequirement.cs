using Unity.Entities;

namespace Waddle.Abilities.Data
{
    public struct AbilityActivationAttributeRequirement : IBufferElementData
    {
        public byte Attribute;
        public float Amount;
    }
}