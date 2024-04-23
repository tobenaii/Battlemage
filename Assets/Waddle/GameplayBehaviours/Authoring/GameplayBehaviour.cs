using Unity.Entities;
using UnityEngine;

namespace Waddle.GameplayBehaviours.Authoring
{
    [DisallowMultipleComponent, RequireComponent(typeof(GameplayBehaviourAuthoring)), ExecuteAlways]
    public abstract class GameplayBehaviour : MonoBehaviour
    {
        protected Baker<GameplayBehaviourAuthoring> Baker { get; private set; }

        internal void Bake(Entity entity, Baker<GameplayBehaviourAuthoring> baker)
        {
            Baker = baker;
            Bake(entity);
        }
        
        protected virtual void Bake(Entity entity)
        {
        }
    }
}
