using Unity.Entities;
using Unity.Mathematics;

namespace Waddle.Runtime.Pathfinding
{
    public struct Waypoint : IBufferElementData
    {
        public float3 Value;
    }
}