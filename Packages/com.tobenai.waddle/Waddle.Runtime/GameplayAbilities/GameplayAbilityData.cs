using Unity.Entities;
using Unity.NetCode;

namespace Waddle.Runtime.GameplayAbilities
{
    [GhostComponent]
    public struct GameplayAbilityData : IComponentData
    {
        [GhostField]
        public Entity Source;
    }
}