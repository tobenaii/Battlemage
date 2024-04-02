using Battlemage.Camera;
using Battlemage.Character;
using Battlemage.Utilities;
using Unity.Burst;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;

namespace Battlemage.Game
{
    public struct CharacterInitialized : IComponentData
    {
    }

    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class CursorInitSystem : SystemBase
    {
        protected override void OnCreate()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        protected override void OnUpdate()
        {
            
        }
    }

    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    [BurstCompile]
    public partial struct ClientInitializeSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            var query = SystemAPI.QueryBuilder().WithAll<CharacterSettings>()
                .WithNone<CharacterInitialized>().Build();
            state.RequireForUpdate(query);
        }

        public void OnUpdate(ref SystemState state)
        {
            EntityCommandBuffer ecb = SystemAPI.GetSingletonRW<BeginSimulationEntityCommandBufferSystem.Singleton>().ValueRW.CreateCommandBuffer(state.WorldUnmanaged);
            foreach (var (character, entity) in SystemAPI.Query<RefRO<CharacterSettings>>()
                         .WithAll<GhostOwnerIsLocal, Child>()
                         .WithNone<CharacterInitialized>()
                         .WithEntityAccess())
            {
                ecb.AddComponent(character.ValueRO.ViewEntity, new MainEntityCamera(75));
                
                // Make local character meshes rendering be shadow-only
                BufferLookup<Child> childBufferLookup = SystemAPI.GetBufferLookup<Child>();
                MiscUtilities.SetShadowModeInHierarchy(state.EntityManager, ecb, entity, ref childBufferLookup, UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly);
                
                ecb.AddComponent<CharacterInitialized>(entity);
            }
        }
    }
}