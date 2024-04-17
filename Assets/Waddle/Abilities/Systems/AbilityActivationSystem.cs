using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using Waddle.Abilities.Data;
using Waddle.Attributes.Data;
using Waddle.Attributes.Extensions;

namespace Waddle.Abilities.Systems
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    public partial struct AbilityActivationSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<NetworkTime>();
        }

        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            var networkTime = SystemAPI.GetSingleton<NetworkTime>();
            foreach (var (abilityActivateRequests, attributes, ghostOwner, entity) in SystemAPI
                         .Query<DynamicBuffer<AbilityActivateRequest>, DynamicBuffer<AttributeMap>, RefRO<GhostOwner>>()
                         .WithAll<Simulate>()
                         .WithEntityAccess()
                         .WithChangeFilter<AbilityActivateRequest>())
            {
                var attributeMap = attributes.AsMap();
                foreach (var request in abilityActivateRequests)
                {
                    var attributeRequirements =
                        SystemAPI.GetBuffer<AbilityActivationAttributeRequirement>(request.AbilityPrefab);
                    bool succeeded = true;
                    foreach (var requirement in attributeRequirements)
                    {
                        var value = attributeMap[requirement.Attribute];
                        if (value.CurrentValue < requirement.Amount)
                        {
                            succeeded = false;
                            break;
                        }
                    }

                    if (succeeded && networkTime.IsFirstTimeFullyPredictingTick)
                    {
                        Debug.Log(state.WorldUnmanaged.IsServer());
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