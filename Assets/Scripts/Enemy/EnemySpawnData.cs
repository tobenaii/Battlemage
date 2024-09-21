using Unity.Entities;
using Unity.Mathematics;

namespace Battlemage.Enemy
{
    public struct EnemySpawnData : IComponentData
    {
        public Entity EnemyPrefab;
        public float3 SpawnPosition;
        public float SpawnDelay;
    }
}