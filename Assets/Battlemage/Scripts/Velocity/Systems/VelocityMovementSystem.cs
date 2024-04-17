using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;

namespace Battlemage.Velocity.Systems
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    public partial struct VelocityMovementSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (velocity, transform) in SystemAPI.Query<RefRO<Data.Velocity>, RefRW<LocalTransform>>().WithAll<Simulate>())
            {
                transform.ValueRW.Position += velocity.ValueRO.Value * SystemAPI.Time.DeltaTime;
            }
        }
    }
}