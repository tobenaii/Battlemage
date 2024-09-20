using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;

namespace Battlemage.SimpleVelocity
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
            
            private void Execute(in Velocity velocity, ref LocalTransform localTransform)
            {
                localTransform.Position += velocity.Value * DeltaTime;
            }
        }
    }
}