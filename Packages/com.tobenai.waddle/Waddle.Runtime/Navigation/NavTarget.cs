using Unity.Entities;
using Unity.Mathematics;

namespace Waddle.Runtime
{
    public struct NavTarget : IComponentData
    {
        public float3 Value;
    }
}