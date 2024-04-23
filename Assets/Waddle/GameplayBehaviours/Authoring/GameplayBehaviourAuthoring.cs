using UnityEngine;

namespace Waddle.GameplayBehaviours.Authoring
{
    [DisallowMultipleComponent, RequireComponent(typeof(GameplayEventsAuthoring)), ExecuteAlways]
    public abstract class GameplayBehaviourAuthoring : MonoBehaviour
    {
    }
}
