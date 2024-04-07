using Battlemage.GameplayBehaviour.Data;
using Battlemage.GameplayBehaviour.Data.GameplayEvents;
using UnityEngine;

namespace Battlemage.GameplayBehaviour.Authoring
{
    [DisallowMultipleComponent, RequireComponent(typeof(GameplayEventsAuthoring)), ExecuteAlways]
    public abstract class GameplayBehaviourAuthoring : MonoBehaviour
    {
        public virtual GameplayOnSpawnEvent.Delegate OnSpawnEvent => default;
        public virtual GameplayOnHitEvent.Delegate OnHitEvent => default;
    }
}
