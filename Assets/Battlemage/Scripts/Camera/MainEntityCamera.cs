using System;
using Unity.Entities;

namespace Battlemage.Camera
{
    [Serializable]
    public struct MainEntityCamera : IComponentData
    {
        public MainEntityCamera(float fov)
        {
            BaseFoV = fov;
            CurrentFoV = fov;
        }
    
        public float BaseFoV;
        public float CurrentFoV;
    }
}
