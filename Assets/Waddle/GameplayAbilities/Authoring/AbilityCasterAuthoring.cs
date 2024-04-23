using Unity.Entities;
using UnityEngine;
using Waddle.Abilities.Data;
using Waddle.GameplayActions.Data;

namespace Waddle.Abilities.Authoring
{
    public class AbilityCasterAuthoring : MonoBehaviour
    {
        public class AbilityCasterAuthoringBaker : Baker<AbilityCasterAuthoring>
        {
            public override void Bake(AbilityCasterAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddBuffer<ActivateAbilityRequest>(entity);
                AddBuffer<GameplayActionRequirement>(entity);
                AddComponent<GameplayActionRequirementResult>(entity);
            }
        }
    }
}