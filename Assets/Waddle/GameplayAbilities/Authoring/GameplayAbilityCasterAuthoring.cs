﻿using Unity.Entities;
using UnityEngine;
using Waddle.GameplayAbilities.Data;
using Waddle.GameplayActions.Data;

namespace Waddle.GameplayAbilities.Authoring
{
    public class GameplayAbilityCasterAuthoring : MonoBehaviour
    {
        public class AbilityCasterAuthoringBaker : Baker<GameplayAbilityCasterAuthoring>
        {
            public override void Bake(GameplayAbilityCasterAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddBuffer<ActivateGameplayAbilityRequest>(entity);
                AddBuffer<GameplayActionRequirement>(entity);
                AddComponent<GameplayActionRequirementResult>(entity);
            }
        }
    }
}