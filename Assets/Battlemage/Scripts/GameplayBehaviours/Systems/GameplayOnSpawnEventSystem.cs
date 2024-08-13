using Battlemage.GameplayBehaviours.Data.GameplayEvents;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Waddle.EntitiesExtended.Groups;
using Waddle.GameplayBehaviours.Data;
using Waddle.GameplayBehaviours.Extensions;

namespace Battlemage.GameplayBehaviours.Systems
{
    [BurstCompile]
    [UpdateInGroup(typeof(BeginSimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    public partial struct GameplayOnSpawnEventSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameplayEventPointer>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            var eventPointers = SystemAPI.GetSingletonBuffer<GameplayEventPointer>();
            var gameplayState = new GameplayState(state.EntityManager, ecb, SystemAPI.Time, state.WorldUnmanaged.IsServer());
            
            foreach (var (eventRefs, entity) in SystemAPI
                         .Query<DynamicBuffer<GameplayEventReference>>()
                         .WithAll<GameplayOnSpawnEvent>()
                         .WithEntityAccess())
            {
                var source = entity;
                var pointer = eventRefs.GetEventPointer(eventPointers, TypeManager.GetTypeInfo<GameplayOnSpawnEvent>().StableTypeHash);
                new FunctionPointer<GameplayOnSpawnEvent.Delegate>(pointer).Invoke(ref gameplayState, ref source);
                ecb.SetComponentEnabled<GameplayOnSpawnEvent>(entity, false);
            }
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}