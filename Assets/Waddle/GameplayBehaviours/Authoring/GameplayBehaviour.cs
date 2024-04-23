using UnityEngine;

namespace Waddle.GameplayBehaviours.Authoring
{
    [DisallowMultipleComponent, RequireComponent(typeof(GameplayBehaviourAuthoring)), ExecuteAlways]
    public abstract class GameplayBehaviour : MonoBehaviour
    {
    }
}
