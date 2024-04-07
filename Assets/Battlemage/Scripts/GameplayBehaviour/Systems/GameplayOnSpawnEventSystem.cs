using System.Runtime.InteropServices;
using Battlemage.GameplayBehaviour.Data;
using Battlemage.GameplayBehaviour.Data.GameplayEvents;
using Unity.Collections;
using Unity.Entities;

namespace Battlemage.GameplayBehaviour.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct GameplayOnSpawnEventSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach (var (onSpawnEvent, entity) in SystemAPI.Query<RefRO<GameplayOnSpawnEvent>>().WithEntityAccess())
            {
                var gameplayState = new GameplayState(ref state, ref ecb);
                var source = entity;
                Marshal.GetDelegateForFunctionPointer<GameplayOnSpawnEvent.Delegate>(onSpawnEvent.ValueRO.EventPointerRef.Value.Pointer).Invoke(ref gameplayState, ref source);
                ecb.RemoveComponent<GameplayOnSpawnEvent>(entity);
            }
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}