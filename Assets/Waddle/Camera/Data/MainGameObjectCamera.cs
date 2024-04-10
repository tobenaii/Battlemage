using UnityEngine;

namespace Waddle.Camera.Data
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