using Unity.Entities;

namespace Waddle.Runtime.GameplayActions
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