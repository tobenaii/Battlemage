using Unity.Entities;
using Unity.Transforms;

namespace Waddle.Runtime.Camera
{
    [UpdateInGroup(typeof(PresentationSystemGroup))]
    public partial class MainCameraSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            if (MainGameObjectCamera.Instance != null && SystemAPI.HasSingleton<MainEntityCamera>())
            {
                var mainEntityCameraEntity = SystemAPI.GetSingletonEntity<MainEntityCamera>();
                var mainEntityCamera = SystemAPI.GetSingleton<MainEntityCamera>();
                var targetLocalToWorld = SystemAPI.GetComponent<LocalToWorld>(mainEntityCameraEntity);
            
                MainGameObjectCamera.Instance.transform.SetPositionAndRotation(targetLocalToWorld.Position, targetLocalToWorld.Rotation);
                MainGameObjectCamera.Instance.fieldOfView = mainEntityCamera.CurrentFoV;
            }
        }
    }
}