using System.Runtime.InteropServices;
using Battlemage.GameplayBehaviours.Data;
using Battlemage.GameplayBehaviours.Data.GameplayEvents;
using Unity.Collections;
using Unity.Entities;
using Waddle.GameplayBehaviour.Data;
using Waddle.GameplayBehaviour.Extensions;
using Waddle.GameplayBehaviour.Utilities;
using Hash128 = Unity.Entities.Hash128;

namespace Battlemage.GameplayBehaviours.Systems
{
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct GameplayOnSpawnEventSystem : ISystem
    {
        private static readonly Hash128 EventHash = GameplayBehaviourUtilities.GetEventHash(typeof(GameplayOnSpawnEvent));

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach (var (eventRefs, entity) in SystemAPI.Query<DynamicBuffer<GameplayEventReference>>()
                         .WithAll<GameplayOnSpawnEvent>()
                         .WithEntityAccess())
            {
                var gameplayState = new GameplayState(state.EntityManager, ref ecb);
                var source = entity;
                var pointer = eventRefs.GetEventPointer(EventHash);
                Marshal.GetDelegateForFunctionPointer<GameplayOnSpawnEvent.Delegate>(pointer).Invoke(ref gameplayState, ref source);
                ecb.RemoveComponent<GameplayOnSpawnEvent>(entity);
            }
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}