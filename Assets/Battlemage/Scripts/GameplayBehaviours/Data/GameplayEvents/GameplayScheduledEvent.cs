using Unity.Entities;
using Waddle.GameplayBehaviour.Data;

namespace Battlemage.GameplayBehaviours.Data.GameplayEvents
{
    [GameplayEventDefinition(typeof(Delegate))]
    public struct GameplayScheduledEvent : IBufferElementData
    {
        public delegate void Delegate(ref GameplayState gameplayState, ref Entity source);
        public Hash128 EventHash;
        public float Time;
    }
}