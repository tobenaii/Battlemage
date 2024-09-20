using Unity.Entities;
using Waddle.Runtime.GameplayBehaviours;

namespace Battlemage.GameplayBehaviours.GameplayEvents
{
    [GameplayEventDefinition(typeof(Delegate))]
    public struct GameplayOnSpawnEvent : IComponentData, IEnableableComponent
    {
        public delegate void Delegate(ref GameplayState gameplayState, ref Entity self);
    }
}