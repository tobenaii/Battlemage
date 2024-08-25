using Unity.Entities;
using Waddle.GameplayBehaviours.Data;

namespace Battlemage.GameplayBehaviours.Data.InputEvents
{
    [GameplayEventDefinition(typeof(Delegate))]
    public struct InputPrimaryAbilityEvent : IComponentData
    {
        public delegate void Delegate(GameplayState gameplayState, Entity source, ButtonState buttonState);
    }
}