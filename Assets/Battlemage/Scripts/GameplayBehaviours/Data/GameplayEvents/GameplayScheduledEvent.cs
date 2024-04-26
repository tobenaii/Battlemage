using Unity.Entities;
using Unity.NetCode;
using Waddle.GameplayBehaviours.Data;

namespace Battlemage.GameplayBehaviours.Data.GameplayEvents
{
    [GameplayEventDefinition(typeof(Delegate))]
    [GhostComponent]
    public struct GameplayScheduledEvent : IBufferElementData
    {
        public delegate void Delegate(ref GameplayState gameplayState, ref Entity source);

        [GhostField] public byte EventIndex;
        [GhostField(Quantization = 0)] public float Countdown;
    }
}