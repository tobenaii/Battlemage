using Unity.Entities;
using Unity.NetCode;

namespace Battlemage.Enemy
{
    [GhostComponent]
    public struct EnemySpawnTimer : IComponentData
    {
        [GhostField]
        public float Timer;
    }
}