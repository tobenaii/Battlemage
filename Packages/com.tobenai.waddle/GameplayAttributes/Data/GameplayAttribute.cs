using Unity.Entities;
using Unity.NetCode;

namespace Waddle.GameplayAttributes.Data
{
    [GhostComponent]
    public struct GameplayAttribute : IBufferElementData
    {
        [GhostField]
        public float BaseValue;
        [GhostField]
        public float CurrentValue;
    }
}