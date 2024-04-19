using Unity.Entities;
using UnityEngine;
using Waddle.GameplayBehaviour.Data;

namespace Waddle.GameplayBehaviour.Authoring
{
    public class GameplayEventsMapperAuthoring : MonoBehaviour
    {
        public class GameplayEventsMapperAuthoringBaker : Baker<GameplayEventsMapperAuthoring>
        {
            public override void Bake(GameplayEventsMapperAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent<InitializeGameplayEvents>(entity);
                AddBuffer<GameplayEventInfoElement>(entity);
                AddBuffer<GameplayEventPointer>(entity);
            }
        }
    }
}