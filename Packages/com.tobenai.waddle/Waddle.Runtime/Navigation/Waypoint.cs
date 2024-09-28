using Unity.Entities;
using Unity.Mathematics;

namespace Waddle.Runtime
{
    public struct Waypoint : IBufferElementData
    {
        public float3 Value;
    }
}