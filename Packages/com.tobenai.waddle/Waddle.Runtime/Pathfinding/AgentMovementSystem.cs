using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace Waddle.Runtime.Pathfinding
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    [UpdateAfter(typeof(AgentPathfindingSystem))]
    public partial struct AgentMovementSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var deltaTime = SystemAPI.Time.DeltaTime;
            foreach (var (navAgent, transform, waypoints) in SystemAPI
                         .Query<RefRW<NavAgent>, RefRW<LocalTransform>, DynamicBuffer<Waypoint>>())
            {
                var index = navAgent.ValueRW.CurrentWaypoint;
                var target = waypoints[index].Value;
                var position = transform.ValueRW.Position;

                position += math.normalize(target - position) * 10f * deltaTime;
                
                if (math.distance(position, target) < 0.1f)
                {
                    index++;
                    index = math.clamp(index, 0, waypoints.Length - 1);
                }
                navAgent.ValueRW.CurrentWaypoint = index;
                transform.ValueRW.Position = position;
            }
        }
    }
}