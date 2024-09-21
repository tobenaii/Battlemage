using Unity.Entities;
using UnityEngine;

namespace Waddle.Runtime.Pathfinding.Authoring
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
                AddBuffer<Waypoint>(entity);
            }
        }
    }
}