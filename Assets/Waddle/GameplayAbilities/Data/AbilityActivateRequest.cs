using Unity.Entities;

namespace Waddle.Abilities.Data
{
    public struct ActivateAbilityRequest : IBufferElementData
    {
        public int RequirementIndices;
        public Entity AbilityPrefab;
    }
}