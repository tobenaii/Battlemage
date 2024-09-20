using Battlemage.Player.Authoring;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;

namespace Battlemage.Player.Systems
{
    [BurstCompile]
    [WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
    public partial struct PlayerSpawnServerSystem : ISystem
    {
        private ComponentLookup<NetworkId> _networkIdFromEntity;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<PlayerSpawner>();
            var builder = new EntityQueryBuilder(Allocator.Temp)
                .WithAll<PlayerJoinRequest>()
                .WithAll<ReceiveRpcCommandRequest>();
            state.RequireForUpdate(state.GetEntityQuery(builder));
            _networkIdFromEntity = state.GetComponentLookup<NetworkId>(true);
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            // Get the prefab to instantiate
            var characterPrefab = SystemAPI.GetSingleton<PlayerSpawner>().Character;
            var playerPrefab = SystemAPI.GetSingleton<PlayerSpawner>().Player;
        
            // Get the name of the prefab being instantiated
            state.EntityManager.GetName(characterPrefab, out var prefabName);
            var worldName = new FixedString32Bytes(state.WorldUnmanaged.Name);

            var commandBuffer = new EntityCommandBuffer(Allocator.Temp);
            _networkIdFromEntity.Update(ref state);

            foreach (var (reqSrc, reqEntity) in SystemAPI.Query<RefRO<ReceiveRpcCommandRequest>>().WithAll<PlayerJoinRequest>().WithEntityAccess())
            {
                commandBuffer.AddComponent<NetworkStreamInGame>(reqSrc.ValueRO.SourceConnection);
                var networkId = _networkIdFromEntity[reqSrc.ValueRO.SourceConnection];

                Debug.Log($"'{worldName}' setting connection '{networkId.Value}' to in game, spawning a Ghost '{prefabName}' for them!");

                var playerCharacter = commandBuffer.Instantiate(characterPrefab);
                commandBuffer.SetComponent(playerCharacter, new GhostOwner { NetworkId = networkId.Value});
                commandBuffer.AppendToBuffer(reqSrc.ValueRO.SourceConnection, new LinkedEntityGroup{Value = playerCharacter});
                commandBuffer.DestroyEntity(reqEntity);

                var playerEntity = commandBuffer.Instantiate(playerPrefab);
                var player = SystemAPI.GetComponent<PlayerController.Data.PlayerController>(playerPrefab);
                player.Character = playerCharacter;
                commandBuffer.SetComponent(playerEntity, player);
                commandBuffer.SetComponent(playerEntity, new GhostOwner { NetworkId = networkId.Value });
            }
            commandBuffer.Playback(state.EntityManager);
        }
    }
}