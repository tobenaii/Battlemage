using Unity.Entities;

namespace Battlemage.GameplayBehaviour.Data
{
    public struct GameplayBehaviourHash : IComponentData
    {
        public Hash128 Value;
    }
}