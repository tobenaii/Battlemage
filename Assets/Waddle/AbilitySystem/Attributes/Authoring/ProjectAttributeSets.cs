using System;
using System.Collections.Generic;
using UnityEngine;

namespace Waddle.AbilitySystem.Attributes.Authoring
{
    [CreateAssetMenu(menuName = "Waddle/AbilitySystem/Project Attributes", fileName = "ProjectAttributes")]
    public class ProjectAttributeSets : ScriptableObject
    {
        [SerializeField] private List<AttributeSet> _attributeSets;

        private void OnValidate()
        {
            byte offset = 0;
            foreach (var attributeSet in _attributeSets)
            {
                foreach (var attribute in attributeSet.Attributes)
                {
                    attribute.SetIndex(offset++);
                }
            }
        }
    }
}