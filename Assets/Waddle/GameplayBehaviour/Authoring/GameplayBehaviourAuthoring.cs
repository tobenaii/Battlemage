using UnityEngine;

namespace Waddle.GameplayBehaviour.Authoring
{
    [DisallowMultipleComponent, RequireComponent(typeof(GameplayEventsAuthoring)), ExecuteAlways]
    public abstract class GameplayBehaviourAuthoring : MonoBehaviour
    {
    }
}
