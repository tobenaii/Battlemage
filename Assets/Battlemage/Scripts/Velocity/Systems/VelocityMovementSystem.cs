using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;

namespace Battlemage.Velocity.Systems
{
    [BurstCompile]
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    public partial struct VelocityMovementSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            state.Dependency = new Job
            {
                DeltaTime = SystemAPI.Time.DeltaTime
            }.ScheduleParallel(state.Dependency);
        }

        [BurstCompile, WithAll(typeof(Simulate))]
        private partial struct Job : IJobEntity
        {
            public float DeltaTime;
            
            private void Execute(in Data.Velocity velocity, ref LocalTransform localTransform)
            {
                localTransform.Position += velocity.Value * DeltaTime;
            }
        }
    }
}