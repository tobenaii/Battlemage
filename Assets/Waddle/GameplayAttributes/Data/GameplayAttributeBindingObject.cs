using Unity.Properties;
using UnityEngine;

namespace Waddle.GameplayAttributes.Data
{
    [CreateAssetMenu(menuName = "Waddle/AbilitySystem/AttributeBindingObject")]
    public class GameplayAttributeBindingObject : ScriptableObject
    {
        [CreateProperty]
        public float Value { get; set; }
    }
}