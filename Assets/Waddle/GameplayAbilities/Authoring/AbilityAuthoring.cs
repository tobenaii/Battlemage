using Unity.Entities;
using UnityEngine;
using Waddle.Abilities.Data;

namespace Waddle.Abilities.Authoring
{
    public class AbilityAuthoring : MonoBehaviour
    {
        public class AbilityAuthoringBaker : Baker<AbilityAuthoring>
        {
            public override void Bake(AbilityAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new AbilityData());
                AddBuffer<AbilityActivationAttributeRequirement>(entity);
            }
        }
    }
}