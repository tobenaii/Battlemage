using Unity.Entities;
using UnityEngine;

namespace Battlemage.Enemy.Authoring
{
    public class EnemySpawnerAuthoring : MonoBehaviour
    {
        [SerializeField] private float _spawnDelay;
        [SerializeField] private GameObject _enemyPrefab;
        
        private class Baker : Baker<EnemySpawnerAuthoring>
        {
            public override void Bake(EnemySpawnerAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.None);
                AddComponent(entity, new EnemySpawnData()
                {
                    EnemyPrefab = GetEntity(authoring._enemyPrefab, TransformUsageFlags.Dynamic),
                    SpawnDelay = authoring._spawnDelay,
                    SpawnPosition = authoring.transform.position
                });
                AddComponent(entity, new EnemySpawnTimer()
                {
                    Timer = authoring._spawnDelay
                });
            }
        }
    }
}