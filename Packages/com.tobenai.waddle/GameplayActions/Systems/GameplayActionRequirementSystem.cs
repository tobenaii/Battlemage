using Unity.Burst;
using Unity.Entities;
using Waddle.GameplayActions.Data;
using Waddle.GameplayAttributes.Data;

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
                         .Query<DynamicBuffer<GameplayActionRequirement>, DynamicBuffer<GameplayAttribute>, RefRW<GameplayActionRequirementResult>>())
            {
                var requirementsRef = requirements;
                for (var i = 0; i < requirements.Length; i++)
                {
                    var requirement = requirements[i];
                    var value = attributes[requirement.Attribute];
                    result.ValueRW.SetResult(i, value.CurrentValue >= requirement.Amount);
                    requirementsRef[i] = requirement;
                }
            }
        }
    }
}