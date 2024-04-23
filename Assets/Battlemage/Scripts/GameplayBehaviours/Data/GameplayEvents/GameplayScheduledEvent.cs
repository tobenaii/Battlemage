using Unity.Entities;
using Waddle.GameplayBehaviours.Data;

namespace Battlemage.GameplayBehaviours.Data.GameplayEvents
{
    [GameplayEventDefinition(typeof(Delegate))]
    public struct GameplayScheduledEvent : IBufferElementData
    {
        public delegate void Delegate(ref GameplayState gameplayState, ref Entity source);
        public ulong TypeHash;
        public int MethodHash;
        public float Time;
        public double TimeStarted;
    }
}