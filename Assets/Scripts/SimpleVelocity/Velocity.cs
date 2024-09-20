using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

namespace Battlemage.SimpleVelocity
{
    [GhostComponent]
    public struct Velocity : IComponentData
    {
        [GhostField]
        public float3 Value;
    }
}