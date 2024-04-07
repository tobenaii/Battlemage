using UnityEngine;

namespace Battlemage.GameplayBehaviour.Authoring
{
    [DisallowMultipleComponent, RequireComponent(typeof(GameplayEventsAuthoring)), ExecuteAlways]
    public abstract class GameplayBehaviourAuthoring : MonoBehaviour
    {
    }
}
