using Unity.Entities;

namespace Battlemage.Player
{
    public struct PlayerAbility : IComponentData
    {
        public Entity PrimaryAbility;
    }
}