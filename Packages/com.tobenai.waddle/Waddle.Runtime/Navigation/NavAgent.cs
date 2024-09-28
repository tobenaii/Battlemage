using Unity.Entities;
using Unity.NetCode;

namespace Waddle.Runtime
{
    [GhostComponent]
    public struct NavAgent : IComponentData
    {
        [GhostField]
        public int CurrentWaypoint;
    }
}