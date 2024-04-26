using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using Waddle.GameplayAbilities.Data;
using Waddle.GameplayActions.Data;
using Waddle.GameplayActions.Systems;

namespace Waddle.GameplayAbilities.Systems
{
    [BurstCompile]
    [UpdateInGroup(typeof(GameplayActionRequestsSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    public partial struct GameplayAbilityActivationRequestSystem : ISystem
    {
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkTime>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var networkTime = SystemAPI.GetSingleton<NetworkTime>();
            var localTransformLookup = SystemAPI.GetComponentLookup<LocalTransform>(true);
            var parentLookup = SystemAPI.GetComponentLookup<Parent>(true);
            var postTransformMatrixLookup = SystemAPI.GetComponentLookup<PostTransformMatrix>(true);
            
            state.Dependency = new Job
            {
                IsServer = state.WorldUnmanaged.IsServer(),
                NetworkTime = networkTime,
                ECB = SystemAPI.GetSingletonRW<BeginSimulationEntityCommandBufferSystem.Singleton>().ValueRW.CreateCommandBuffer(state.WorldUnmanaged),
                LocalTransformLookup = localTransformLookup,
                ParentLookup = parentLookup,
                PostTransformMatrixLookup = postTransformMatrixLookup
            }.Schedule(state.Dependency);
        }

        [BurstCompile]
        [WithAll(typeof(Simulate))]
        private partial struct Job : IJobEntity
        {
            public bool IsServer;
            public NetworkTime NetworkTime;
            public EntityCommandBuffer ECB;
            [ReadOnly]
            public ComponentLookup<LocalTransform> LocalTransformLookup;
            [ReadOnly]
            public ComponentLookup<Parent> ParentLookup;
            [ReadOnly]
            public ComponentLookup<PostTransformMatrix> PostTransformMatrixLookup;
            
            private void Execute(Entity entity, in GameplayAbilityCasterData abilityCasterData,
                ref DynamicBuffer<ActivateGameplayAbilityRequest> abilityActivateRequests,
                in GameplayActionRequirementResult requirementResult, in GhostOwner ghostOwner)
            {
                foreach (var request in abilityActivateRequests)
                {
                    var succeeded = requirementResult.HasSucceeded(request.RequirementIndices);
                    if (succeeded && NetworkTime.IsFirstTimeFullyPredictingTick)
                    {
                        var ability = ECB.Instantiate(request.AbilityPrefab);
                        TransformHelpers.ComputeWorldTransformMatrix(abilityCasterData.AbilitySpawnPoint, out var abilitySpawn, ref LocalTransformLookup, ref ParentLookup, ref PostTransformMatrixLookup);
                        ECB.SetComponent(ability, new LocalTransform()
                        {
                            Position = abilitySpawn.Translation(),
                            Rotation = abilitySpawn.Rotation(),
                            Scale = 1.0f
                        });
                        ECB.SetComponent(ability, new GameplayAbilityData()
                        {
                            Source = entity
                        });
                        if (IsServer)
                        {
                            ECB.SetComponent(ability, new GhostOwner()
                            {
                                NetworkId = ghostOwner.NetworkId
                            });
                        }
                    }
                }
                abilityActivateRequests.Clear();
            }
        }
    }
}