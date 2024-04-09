using Unity.Entities;

namespace Battlemage.GameplayBehaviour.Data.GameplayEvents
{
    [GameplayEventDefinition(typeof(Delegate))]
    public struct GameplayOnSpawnEvent : IComponentData
    {
        public delegate void Delegate(ref GameplayState gameplayState, ref Entity self);
    }
}