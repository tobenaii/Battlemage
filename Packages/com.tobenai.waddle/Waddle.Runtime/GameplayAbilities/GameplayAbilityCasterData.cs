using Unity.Entities;

namespace Waddle.Runtime.GameplayAbilities
{
    public struct GameplayAbilityCasterData : IComponentData
    {
        public Entity AbilitySpawnPoint;
    }
}