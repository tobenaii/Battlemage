using Unity.Entities;

namespace Waddle.GameplayAbilities.Data
{
    public struct GameplayAbilityCasterData : IComponentData
    {
        public Entity AbilitySpawnPoint;
    }
}