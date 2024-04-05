using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Waddle.AbilitySystem.GameplayEffects.Data;

namespace Waddle.AbilitySystem.GameplayEffects.Authoring
{
    public class ApplyGameplayEffectsOnActivateAuthoring : MonoBehaviour
    {
        [SerializeField] private List<GameplayEffectAuthoring> _gameplayEffects;

        public class ApplyGameplayEffectsBaker : Baker<ApplyGameplayEffectsOnActivateAuthoring>
        {
            public override void Bake(ApplyGameplayEffectsOnActivateAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);

                AddGameplayEffectBuffer<OnAbilityActivateGameplayEffect>(entity, authoring._gameplayEffects);
            }
            
            private void AddGameplayEffectBuffer<T>(Entity entity, List<GameplayEffectAuthoring> gameplayEffects) where T : unmanaged, IBufferElementData, IGameplayEffectReference
            {
                var buffer = AddBuffer<T>(entity);
                foreach (var gameplayEffect in gameplayEffects)
                {
                    buffer.Add(new T
                    {
                        GameplayEffect = gameplayEffect.Bake(this)
                    });
                }
            }
        }
    }
}