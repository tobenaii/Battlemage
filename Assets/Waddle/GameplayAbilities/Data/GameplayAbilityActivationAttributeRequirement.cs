using Unity.Entities;

namespace Waddle.GameplayAbilities.Data
{
    public struct GameplayAbilityActivationAttributeRequirement : IBufferElementData
    {
        public byte Attribute;
        public float Amount;
    }
}