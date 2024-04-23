using Unity.Entities;
using Waddle.GameplayAbilities.Data;

namespace Waddle.GameplayAbilities.Authoring
{
    public abstract class AbilityBehaviour : GameplayBehaviours.Authoring.GameplayBehaviour
    {
        protected override void Bake(Entity entity)
        {
            Baker.AddComponent(entity, new GameplayAbilityData());
            Baker.AddBuffer<GameplayAbilityActivationAttributeRequirement>(entity);
        }
    }
}