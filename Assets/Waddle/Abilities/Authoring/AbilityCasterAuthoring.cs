using Unity.Entities;
using UnityEngine;
using Waddle.Abilities.Data;

namespace Waddle.Abilities.Authoring
{
    public class AbilityCasterAuthoring : MonoBehaviour
    {
        public class AbilityCasterAuthoringBaker : Baker<AbilityCasterAuthoring>
        {
            public override void Bake(AbilityCasterAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddBuffer<AbilityActivateRequest>(entity);
            }
        }
    }
}