using Unity.Entities;

namespace Battlemage.Enemy
{
    public struct EnemySpawnTimer : IComponentData
    {
        public float Timer;
    }
}