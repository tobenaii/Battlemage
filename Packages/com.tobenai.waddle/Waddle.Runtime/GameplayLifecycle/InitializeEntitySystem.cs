using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Entities;

namespace Waddle.Runtime.GameplayLifecycle
{
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderLast = true)]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    public partial struct InitializeEntitySystem : ISystem
    {
        /// <inheritdoc/>
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var query = SystemAPI.QueryBuilder().WithAll<InitializeEntity>().Build();

            state.Dependency = new MarkInitializedJob
                {
                    InitializeEntityHandle = SystemAPI.GetComponentTypeHandle<InitializeEntity>(),
                }
                .ScheduleParallel(query, state.Dependency);
        }

        [BurstCompile]
        private struct MarkInitializedJob : IJobChunk
        {
            public ComponentTypeHandle<InitializeEntity> InitializeEntityHandle;

            public void Execute(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask,
                in v128 chunkEnabledMask)
            {
                chunk.SetComponentEnabledForAll(ref InitializeEntityHandle, false);
            }
        }
    }
}