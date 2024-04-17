using Unity.Entities;
using Unity.Mathematics;

namespace Battlemage.Velocity.Data
{
    public struct Velocity : IComponentData
    {
        public float3 Value;
    }
}