using Unity.Entities;

namespace Waddle.GameplayAbilities.Data
{
    public struct ActivateGameplayAbilityRequest : IBufferElementData
    {
        public int RequirementIndices;
        public Entity AbilityPrefab;
    }
}