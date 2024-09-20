using Unity.Entities;
using Waddle.GameplayActions.Data;

namespace Waddle.GameplayActions.Systems
{
    [UpdateInGroup(typeof(GameplayActionRequestsSystemGroup), OrderLast = true)]
    public partial struct ClearGameplayActionRequirementsSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            foreach (var requirements in SystemAPI.Query<DynamicBuffer<GameplayActionRequirement>>())
            {
                requirements.Clear();
            }
        }
    }
}