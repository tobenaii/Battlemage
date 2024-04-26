using Unity.Entities;
using Waddle.GameplayBehaviours.Data;

namespace Battlemage.GameplayBehaviours.Data.GameplayEvents
{
    [GameplayEventDefinition(typeof(Delegate))]
    public struct GameplayOnHitEvent : IBufferElementData
    {
        public delegate void Delegate(ref GameplayState gameplayState, ref Entity source, ref Entity target);

        public short EventIndex;
        public float Radius;
    }
}