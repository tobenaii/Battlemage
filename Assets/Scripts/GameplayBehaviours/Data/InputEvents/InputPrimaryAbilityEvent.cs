using Unity.Entities;
using Waddle.GameplayBehaviours.Data;

namespace Battlemage.GameplayBehaviours.Data.InputEvents
{
    [GameplayEventDefinition(typeof(Delegate))]
    public struct InputPrimaryAbilityEvent : IComponentData
    {
        public delegate void Delegate(ref GameplayState gameplayState, ref Entity source, ref ButtonState buttonState);
    }
}