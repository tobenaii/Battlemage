using Unity.Entities;
using Waddle.GameplayBehaviours.Data;

namespace Battlemage.GameplayBehaviours.Data.GameplayEvents
{
    [GameplayEventDefinition(typeof(Delegate))]
    public struct GameplayOnSpawnEvent : IComponentData
    {
        public delegate void Delegate(ref GameplayState gameplayState, ref Entity self);
    }
}