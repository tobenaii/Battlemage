using Unity.Burst;
using Unity.Entities;
using Waddle.GameplayActions.Data;
using Waddle.GameplayAttributes.Data;
using Waddle.GameplayAttributes.Extensions;

namespace Waddle.GameplayActions.Systems
{
    [BurstCompile]
    [UpdateInGroup(typeof(GameplayActionRequestsSystemGroup), OrderFirst = true)]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    public partial struct GameplayActionRequirementSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            foreach (var (requirements, attributes, result) in SystemAPI
                         .Query<DynamicBuffer<GameplayActionRequirement>, DynamicBuffer<GameplayAttributeMap>, RefRW<GameplayActionRequirementResult>>())
            {
                var attributeMap = attributes.AsMap();
                for (var i = 0; i < requirements.Length; i++)
                {
                    var requirement = requirements[i];
                    var value = attributeMap[requirement.Attribute];
                    result.ValueRW.SetResult(i, value.CurrentValue >= requirement.Amount);
                    requirements[i] = requirement;
                }
            }
        }
    }
}