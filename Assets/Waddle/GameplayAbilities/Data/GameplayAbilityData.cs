using Unity.Entities;
using Unity.NetCode;

namespace Waddle.GameplayAbilities.Data
{
    [GhostComponent]
    public struct GameplayAbilityData : IComponentData
    {
        [GhostField]
        public Entity Source;
    }
}