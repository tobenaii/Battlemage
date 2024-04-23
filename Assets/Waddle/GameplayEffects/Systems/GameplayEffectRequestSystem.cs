using Unity.Entities;
using Waddle.GameplayEffects.Data;

namespace Waddle.GameplayEffects.Systems
{
    public partial struct GameplayEffectRequestSystem : ISystem
    {
        private Entity _effectPrefab;
        
        public void OnCreate(ref SystemState state)
        {
            _effectPrefab = state.EntityManager.CreateEntity(
                ComponentType.ReadWrite<Prefab>(),
                ComponentType.ReadWrite<GameplayEffects.Data.GameplayEffect>(),
                ComponentType.ReadWrite<GameplayAttributeModifier>(), 
                ComponentType.ReadWrite<GameplayTagModifier>());
        }

        public void OnUpdate(ref SystemState state)
        {
            
        }
    }
}