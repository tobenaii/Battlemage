using UnityEngine;

namespace Battlemage.Camera
{
    public class MainGameObjectCamera : MonoBehaviour
    {
        public static UnityEngine.Camera Instance;

        void Awake()
        {
            Instance = GetComponent<UnityEngine.Camera>();
        }
    }
}