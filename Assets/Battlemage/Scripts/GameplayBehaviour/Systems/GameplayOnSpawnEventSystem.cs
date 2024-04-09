using System.Runtime.InteropServices;
using Battlemage.GameplayBehaviour.Data;
using Battlemage.GameplayBehaviour.Data.GameplayEvents;
using Battlemage.GameplayBehaviour.Extensions;
using Battlemage.GameplayBehaviour.Utilities;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Hash128 = Unity.Entities.Hash128;

namespace Battlemage.GameplayBehaviour.Systems
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
                var gameplayState = new GameplayState(ref state, ref ecb);
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