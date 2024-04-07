using Battlemage.GameplayBehaviour.Data;
using UnityEngine;

namespace Battlemage.GameplayBehaviour.Authoring
{
    [DisallowMultipleComponent, RequireComponent(typeof(GameplayEventsAuthoring)), ExecuteAlways]
    public abstract class GameplayBehaviourAuthoring : MonoBehaviour
    {
        public virtual GameplayOnHitEvent.Delegate OnHitCallback => default;
    }
}
