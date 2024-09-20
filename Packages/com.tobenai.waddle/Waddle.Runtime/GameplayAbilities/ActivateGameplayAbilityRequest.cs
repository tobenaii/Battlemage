using Unity.Entities;

namespace Waddle.Runtime.GameplayAbilities
{
    public struct ActivateGameplayAbilityRequest : IBufferElementData
    {
        public int RequirementIndices;
        public Entity AbilityPrefab;
    }
}