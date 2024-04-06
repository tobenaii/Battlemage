using Battlemage.GameplayBehaviour.Data;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

namespace Battlemage.GameplayBehaviour.Authoring
{
    [DisallowMultipleComponent, RequireComponent(typeof(GameplayBehaviourAuthoring))]
    public class GameplayEventsAuthoring : MonoBehaviour
    {
        public class GameplayEventsAuthoringBaker : Baker<GameplayEventsAuthoring>
        {
            public override void Bake(GameplayEventsAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                var gameplayBehaviour = GetComponent<GameplayBehaviourAuthoring>();
                
                if (gameplayBehaviour.OnHitCallback != default)
                {
                    AddComponent(entity, new GameplayOnHitCallback()
                    {
                        Callback = BurstCompiler.CompileFunctionPointer(gameplayBehaviour.OnHitCallback).Value.ToInt64()
                    });
                }
            }
        }
    }
}