using Unity.Entities;

namespace Waddle.Runtime.GameplayAbilities
{
    public struct GameplayAbilityActivationAttributeRequirement : IBufferElementData
    {
        public byte Attribute;
        public float Amount;
    }
}