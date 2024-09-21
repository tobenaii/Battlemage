using Unity.Entities;
using Unity.Mathematics;

namespace Waddle.Runtime.Pathfinding
{
    public struct NavTarget : IComponentData
    {
        public float3 Value;
    }
}