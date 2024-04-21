using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Waddle.Abilities.Data;
using Waddle.GameplayActions.Data;
using Waddle.GameplayActions.Systems;

namespace Waddle.Abilities.Systems
{
    [UpdateInGroup(typeof(GameplayActionRequestsSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    public partial struct AbilityActivationRequestSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkTime>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            var networkTime = SystemAPI.GetSingleton<NetworkTime>();
            foreach (var (abilityActivateRequests, requirementResult, ghostOwner, entity) in SystemAPI
                         .Query<DynamicBuffer<ActivateAbilityRequest>, RefRO<GameplayActionRequirementResult>, RefRO<GhostOwner>>()
                         .WithAll<Simulate>()
                         .WithEntityAccess()
                         .WithChangeFilter<ActivateAbilityRequest>())
            {
                foreach (var request in abilityActivateRequests)
                {
                    var succeeded = requirementResult.ValueRO.HasSucceeded(request.RequirementIndices);
                    if (succeeded && networkTime.IsFirstTimeFullyPredictingTick)
                    {
                        var ability = ecb.Instantiate(request.AbilityPrefab);
                        ecb.SetComponent(ability, new AbilityData()
                        {
                            Source = entity
                        });
                        if (state.WorldUnmanaged.IsServer())
                        {
                            ecb.SetComponent(ability, new GhostOwner()
                            {
                                NetworkId = ghostOwner.ValueRO.NetworkId
                            });
                        }
                    }
                }
                abilityActivateRequests.Clear();
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}