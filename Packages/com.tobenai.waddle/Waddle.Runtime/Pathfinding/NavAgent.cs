using Unity.Entities;
using Unity.Mathematics;

namespace Waddle.Runtime.Pathfinding
{
    public struct NavAgent : IComponentData
    {
        public int CurrentWaypoint;
    }
}