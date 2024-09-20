using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using Waddle.Runtime.Camera;
using Waddle.Runtime.FirstPersonCharacter;

namespace Battlemage.Player
{
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    [BurstCompile]
    public partial struct PlayerCharacterSetupSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            var query = SystemAPI.QueryBuilder().WithAll<CharacterSettings>()
                .WithNone<CharacterSetupTag>().Build();
            state.RequireForUpdate(query);
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = SystemAPI.GetSingletonRW<BeginSimulationEntityCommandBufferSystem.Singleton>().ValueRW.CreateCommandBuffer(state.WorldUnmanaged);
            foreach (var (character, entity) in SystemAPI.Query<RefRO<CharacterSettings>>()
                         .WithAll<GhostOwnerIsLocal, Child>()
                         .WithNone<CharacterSetupTag>()
                         .WithEntityAccess())
            {
                ecb.AddComponent(character.ValueRO.ViewEntity, new MainEntityCamera(75));
                ecb.AddComponent(entity, new PlayerCharacterTag());
                
                // Make local character meshes rendering be shadow-only
                BufferLookup<Child> childBufferLookup = SystemAPI.GetBufferLookup<Child>();
                CharacterUtilities.SetShadowModeInHierarchy(state.EntityManager, ecb, entity, ref childBufferLookup, UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly);
                
                ecb.AddComponent<CharacterSetupTag>(entity);
            }
        }
    }
}