using Unity.Entities;
using Unity.Mathematics;
using Waddle.GameplayBehaviours.Data;

namespace Battlemage.GameplayBehaviours.Data.InputEvents
{
    [GameplayEventDefinition(typeof(Delegate))]
    public struct InputMoveEvent : IComponentData
    {
        public delegate void Delegate(GameplayState gameplayState, Entity source, float2 value);
    }
}