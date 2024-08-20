using Unity.Entities;
using Unity.NetCode;
using Waddle.EntitiesExtended.Groups;
using Waddle.GameplayEffects.Data;

namespace Waddle.GameplayEffects.Systems
{
    [UpdateInGroup(typeof(BeginSimulationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    public partial struct GameplayEffectSetupSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            var prefab = state.EntityManager.CreateEntity(
                ComponentType.ReadOnly<Prefab>(),
                ComponentType.ReadOnly<GameplayEffect>(),
                ComponentType.ReadOnly<GameplayAttributeModifier>(),
                ComponentType.ReadOnly<GameplayTagModifier>()
            );
            
            state.EntityManager.CreateSingleton(new GameplayEffectPrefab()
            {
                Value = prefab
            });
        }
    }
}