using Unity.Burst;
using Unity.Entities;

namespace Waddle.Runtime.GameplayLifecycle
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [UpdateAfter(typeof(InstantiateCommandBufferSystem))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    public partial struct DestroyEntitySystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var query = Unity.NetCode.ClientServerWorldExtensions.IsClient(state.WorldUnmanaged)
                ? SystemAPI.QueryBuilder().WithAll<DestroyEntity>().WithNone<Unity.NetCode.GhostInstance>().Build()
                : SystemAPI.QueryBuilder().WithAll<DestroyEntity>().Build();

            if (query.IsEmpty)
            {
                return;
            }

            foreach (var e in query.ToEntityArray(state.WorldUpdateAllocator))
            {
                state.EntityManager.DestroyEntity(e);
            }
        }
    }
}