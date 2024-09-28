using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Unity.Transforms;

namespace Waddle.Runtime
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [UpdateAfter(typeof(AgentPathfindingSystem))]
    public partial struct AgentMovementSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;
            foreach (var (navAgent, transform, waypoints) in SystemAPI
                         .Query<RefRW<NavAgent>, RefRW<LocalTransform>, DynamicBuffer<Waypoint>>()
                         .WithAll<Simulate>())
            {
                ref var index = ref navAgent.ValueRW.CurrentWaypoint;
                if (index == -1) continue;
                var target = waypoints[index].Value;
                var position = transform.ValueRO.Position;
                
                if (math.distance(position, target) < 0.1f)
                {
                    index++;
                    if (index == waypoints.Length)
                    {
                        index = -1;
                    }
                }
                var direction = math.normalize(target - position);
                transform.ValueRW.Position += direction * 10 * deltaTime;
            }
        }
    }
}