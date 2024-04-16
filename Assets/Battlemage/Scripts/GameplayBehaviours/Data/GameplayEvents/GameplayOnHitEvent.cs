using Unity.Entities;
using Waddle.GameplayBehaviour.Data;

namespace Battlemage.GameplayBehaviours.Data.GameplayEvents
{
    [GameplayEventDefinition(typeof(GameplayOnHitEvent.Delegate))]
    public struct GameplayOnHitEvent : IComponentData
    {
        public delegate void Delegate(ref GameplayState gameplayState, ref Entity source, ref Entity target);
    }
}