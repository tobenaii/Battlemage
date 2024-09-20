using Unity.Entities;
using Unity.Mathematics;
using Waddle.Runtime.GameplayBehaviours;

namespace Battlemage.GameplayBehaviours.InputEvents
{
    [GameplayEventDefinition(typeof(Delegate))]
    public struct InputLookEvent : IComponentData
    {
        public delegate void Delegate(ref GameplayState gameplayState, ref Entity source, ref float2 value);
    }
}