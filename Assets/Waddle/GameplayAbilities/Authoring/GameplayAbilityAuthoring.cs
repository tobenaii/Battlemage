using Unity.Entities;
using UnityEngine;
using Waddle.GameplayAbilities.Data;

namespace Waddle.GameplayAbilities.Authoring
{
    public class GameplayAbilityAuthoring : MonoBehaviour
    {
        public class AbilityAuthoringBaker : Baker<GameplayAbilityAuthoring>
        {
            public override void Bake(GameplayAbilityAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new GameplayAbilityData());
                AddBuffer<GameplayAbilityActivationAttributeRequirement>(entity);
            }
        }
    }
}