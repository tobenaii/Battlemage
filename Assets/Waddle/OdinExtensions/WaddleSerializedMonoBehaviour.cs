using Sirenix.OdinInspector;

namespace Waddle.OdinExtensions
{
    public class WaddleSerializedMonoBehaviour : 
#if UNITY_EDITOR
        SerializedMonoBehaviour
#else
        MonoBehaviour
#endif
    {
        
    }
}