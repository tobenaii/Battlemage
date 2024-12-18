﻿using Unity.Entities;
using Waddle.Runtime.GameplayActions;
using Waddle.Runtime.GameplayBehaviours;

namespace Waddle.Runtime.GameplayAbilities
{
    public static class GameplayStateExtensions
    {
        public static void TryActivateAbility(this GameplayState gameplayState, Entity entity, Entity abilityPrefab)
        {
            var abilityRequirements = gameplayState.GetBuffer<GameplayAbilityActivationAttributeRequirement>(abilityPrefab);
            var requirements = gameplayState.GetBuffer<GameplayActionRequirement>(entity);
            var requests = gameplayState.GetBuffer<ActivateGameplayAbilityRequest>(entity);
            var requirementIndices = 0;
            
            foreach (var abilityRequirement in abilityRequirements)
            {
                requirements.Add(new GameplayActionRequirement()
                {
                    Attribute = abilityRequirement.Attribute,
                    Amount = abilityRequirement.Amount
                });
            
                requirementIndices |= (1 << requirements.Length - 1);
            }
            requests.Add(new ActivateGameplayAbilityRequest()
            {
                AbilityPrefab = abilityPrefab,
                RequirementIndices = requirementIndices
            });
        }
    }
}