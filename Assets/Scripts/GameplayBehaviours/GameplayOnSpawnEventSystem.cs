using Battlemage.GameplayBehaviours.GameplayEvents;
using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using Waddle.Runtime.GameplayBehaviours;
using Waddle.Runtime.GameplayLifecycle;

namespace Battlemage.GameplayBehaviours
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [BurstCompile]
    public partial struct GameplayOnSpawnEventSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var instantiateEcb = SystemAPI.GetSingletonRW<InstantiateCommandBufferSystem.Singleton>().ValueRW
                .CreateCommandBuffer(state.WorldUnmanaged);
            var gameplayState = new GameplayState(state.EntityManager, instantiateEcb, SystemAPI.Time, state.WorldUnmanaged.IsServer());
            
            foreach (var (eventRefs, entity) in SystemAPI
                         .Query<DynamicBuffer<GameplayEventReference>>()
                         .WithAll<GameplayOnSpawnEvent, InitializeEntity>()
                         .WithEntityAccess())
            {
                var source = entity;
                var pointer = eventRefs.GetEventPointer(TypeManager.GetTypeInfo<GameplayOnSpawnEvent>().StableTypeHash);
                new FunctionPointer<GameplayOnSpawnEvent.Delegate>(pointer).Invoke(ref gameplayState, ref source);
            }
        }
    }
}