﻿using Unity.Entities;
using UnityEngine;
using Waddle.Runtime.GameplayAbilities;
using Waddle.Runtime.GameplayActions;

namespace Waddle.Authoring.GameplayAbilities
{
    public class GameplayAbilityCasterAuthoring : MonoBehaviour
    {
        [SerializeField] private Transform _abilitySpawnPoint;
        
        public class AbilityCasterAuthoringBaker : Baker<GameplayAbilityCasterAuthoring>
        {
            public override void Bake(GameplayAbilityCasterAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddBuffer<ActivateGameplayAbilityRequest>(entity);
                AddBuffer<GameplayActionRequirement>(entity);
                AddComponent<GameplayActionRequirementResult>(entity);
                AddComponent(entity, new GameplayAbilityCasterData()
                {
                    AbilitySpawnPoint = GetEntity(authoring._abilitySpawnPoint, TransformUsageFlags.Dynamic) 
                });
            }
        }
    }
}