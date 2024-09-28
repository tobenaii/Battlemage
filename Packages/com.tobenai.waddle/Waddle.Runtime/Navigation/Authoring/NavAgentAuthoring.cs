using Unity.Entities;
using UnityEngine;
using Waddle.Runtime;
using Waddle.Runtime.GameplayLifecycle;

namespace Waddle.Authoring
{
    public class NavAgentAuthoring : MonoBehaviour
    {
        private class Baker : Baker<NavAgentAuthoring>
        {
            public override void Bake(NavAgentAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
                AddComponent(entity, new NavAgent());
                AddComponent(entity, new NavTarget());
                AddComponent(entity, new DestroyEntity());
                SetComponentEnabled<DestroyEntity>(entity, false);
                AddBuffer<Waypoint>(entity);
            }
        }
    }
}