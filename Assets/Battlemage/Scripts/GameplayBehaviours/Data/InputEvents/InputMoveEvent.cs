using Unity.Entities;
using Unity.Mathematics;
using Waddle.GameplayBehaviour.Data;

namespace Battlemage.GameplayBehaviours.Data.InputEvents
{
    [GameplayEventDefinition(typeof(Delegate))]
    public struct InputMoveEvent : IComponentData
    {
        public delegate void Delegate(ref GameplayState gameplayState, ref Entity source, ref float2 value);
    }
}