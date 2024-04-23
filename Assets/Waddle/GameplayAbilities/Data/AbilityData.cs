using Unity.Entities;
using Unity.NetCode;

namespace Waddle.Abilities.Data
{
    [GhostComponent]
    public struct AbilityData : IComponentData
    {
        [GhostField]
        public Entity Source;
    }
}