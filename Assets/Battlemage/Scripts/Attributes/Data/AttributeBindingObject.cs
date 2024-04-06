using Unity.Properties;
using UnityEngine;

namespace Waddle.AbilitySystem.Attributes.Data
{
    [CreateAssetMenu(menuName = "Waddle/AbilitySystem/AttributeBindingObject")]
    public class AttributeBindingObject : ScriptableObject
    {
        [CreateProperty]
        public float Value { get; set; }
    }
}