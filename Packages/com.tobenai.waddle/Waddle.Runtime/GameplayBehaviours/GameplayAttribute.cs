using Unity.Entities;
using Unity.NetCode;

namespace Waddle.Runtime.GameplayAttributes
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