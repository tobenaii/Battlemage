using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

namespace Battlemage.SimpleVelocity.Data
{
    [GhostComponent]
    public struct Velocity : IComponentData
    {
        [GhostField]
        public float3 Value;
    }
}