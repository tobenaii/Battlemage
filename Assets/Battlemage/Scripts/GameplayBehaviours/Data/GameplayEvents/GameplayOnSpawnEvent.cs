using Unity.Entities;
using Unity.NetCode;
using Waddle.GameplayBehaviours.Data;

namespace Battlemage.GameplayBehaviours.Data.GameplayEvents
{
    [GhostEnabledBit]
    [GameplayEventDefinition(typeof(Delegate))]
    public struct GameplayOnSpawnEvent : IComponentData, IEnableableComponent
    {
        public delegate void Delegate(ref GameplayState gameplayState, ref Entity self);
    }
}