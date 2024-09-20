using Unity.Entities;
using UnityEngine;
using Waddle.Runtime.Camera;

namespace Waddle.Authoring.Camera
{
    [DisallowMultipleComponent]
    public class MainEntityCameraAuthoring : MonoBehaviour
    {
        [SerializeField] private float _fov = 75f;

        public class Baker : Baker<MainEntityCameraAuthoring>
        {
            public override void Bake(MainEntityCameraAuthoring authoring)
            { 
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                AddComponent(entity, new MainEntityCamera(authoring._fov));
            }
        }
    }
}
