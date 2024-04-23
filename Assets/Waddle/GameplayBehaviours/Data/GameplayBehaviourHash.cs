using Unity.Entities;

namespace Waddle.GameplayBehaviours.Data
{
    public struct GameplayBehaviourHash : IComponentData
    {
        public Hash128 Value;
    }
}