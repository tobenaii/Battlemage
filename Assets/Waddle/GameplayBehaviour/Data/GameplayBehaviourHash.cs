using Unity.Entities;

namespace Waddle.GameplayBehaviour.Data
{
    public struct GameplayBehaviourHash : IComponentData
    {
        public Hash128 Value;
    }
}