using Unity.Properties;
using UnityEngine;

namespace Waddle.Runtime.GameplayAttributes
{
    [CreateAssetMenu(menuName = "Waddle/AbilitySystem/AttributeBindingObject")]
    public class GameplayAttributeBindingObject : ScriptableObject
    {
        [CreateProperty]
        public float Value { get; set; }
    }
}