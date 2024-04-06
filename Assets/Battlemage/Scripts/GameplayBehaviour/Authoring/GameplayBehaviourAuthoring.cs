using Battlemage.GameplayBehaviour.Data;
using UnityEngine;

namespace Battlemage.GameplayBehaviour.Authoring
{
    [DisallowMultipleComponent, RequireComponent(typeof(GameplayEventsAuthoring))]
    public abstract class GameplayBehaviourAuthoring : MonoBehaviour
    {
        public virtual GameplayOnHitCallback.Delegate OnHitCallback => default;

    }
}