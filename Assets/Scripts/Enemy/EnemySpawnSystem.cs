using Unity.Entities;
using Unity.Transforms;
using Waddle.Runtime.GameplayLifecycle;

namespace Battlemage.Enemy
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct EnemySpawnSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingletonRW<InstantiateCommandBufferSystem.Singleton>().ValueRW
                .CreateCommandBuffer(state.WorldUnmanaged);
            var deltaTime = SystemAPI.Time.DeltaTime;
            
            foreach (var (spawnData, spawnTimer) in SystemAPI.Query<RefRO<EnemySpawnData>, RefRW<EnemySpawnTimer>>())
            {
                var timer = spawnTimer.ValueRO.Timer;
                timer -= deltaTime;
                if (timer <= 0)
                {
                    var enemy = ecb.Instantiate(spawnData.ValueRO.EnemyPrefab);
                    ecb.SetComponent(enemy, new LocalTransform()
                    {
                        Position = spawnData.ValueRO.SpawnPosition
                    });
                    timer = spawnData.ValueRO.SpawnDelay;
                }

                spawnTimer.ValueRW.Timer = timer;
            }
        }
    }
}