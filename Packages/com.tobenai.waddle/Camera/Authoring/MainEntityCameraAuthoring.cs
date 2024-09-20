using Unity.Entities;
using UnityEngine;
using Waddle.Camera.Data;

namespace Waddle.Camera.Authoring
{
    [DisallowMultipleComponent]
    public class MainEntityCameraAuthoring : MonoBehaviour
    {
        [SerializeField] private float _fov = 75f;

        public class Baker : Baker<MainEntityCameraAuthoring>
        {
            public override void Bake(MainEntityCameraAuthoring authoring)
            { 
                Entity entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new MainEntityCamera(authoring._fov));
            }
        }
    }
}
