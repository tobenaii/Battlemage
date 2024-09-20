using Unity.Entities;
using Waddle.Runtime.GameplayBehaviours;

namespace Battlemage.GameplayBehaviours.InputEvents
{
    [GameplayEventDefinition(typeof(Delegate))]
    public struct InputJumpEvent : IComponentData
    {
        public delegate void Delegate(ref GameplayState gameplayState, ref Entity source, ref ButtonState buttonState);
    }
}