using System;
using System.Collections.Generic;
using System.Linq;
using BovineLabs.Core.Iterators;
using Sirenix.OdinInspector;
using Unity.Entities;
using UnityEngine;
using Waddle.AbilitySystem.Attributes.Data;

namespace Waddle.AbilitySystem.Attributes.Authoring
{
    public class AttributeSetAuthoring : MonoBehaviour
    {
        [Serializable]
        private class AttributeSetWithValues
        {
            [SerializeField] 
            private AttributeSet _attributeSet;
            
            [SerializeField]
            [ShowIf(nameof(_attributeSet))]
            [ListDrawerSettings(HideAddButton = true, HideRemoveButton = true)] 
            private List<AttributeSetValue> _attributeValues = new();
            
            public AttributeSet AttributeSet => _attributeSet;

            public float GetDefaultValue(WaddleAttribute attribute)
            {
                return _attributeValues.First(x => x.Attribute == attribute).Value;
            }
            
            public void Update()
            {
                if (_attributeSet == null)
                {
                    _attributeValues.Clear();
                    return;
                }
                
                foreach (var attribute in _attributeSet.Attributes)
                {
                    if (_attributeValues.Find(x => x.Attribute == attribute) == null)
                    {
                        _attributeValues.Add(new AttributeSetValue(attribute));
                    }
                }
            }
            
            [Serializable]
            private class AttributeSetValue
            {
                [HorizontalGroup]
                [SerializeField, ReadOnly, HideLabel] private WaddleAttribute _attribute;
                [HorizontalGroup]
                [SerializeField, HideLabel] private float _value;

                public WaddleAttribute Attribute => _attribute;
                public float Value => _value;

                public AttributeSetValue(WaddleAttribute attribute)
                {
                    _attribute = attribute;
                }
            }
        }
        
        
        [SerializeField] private List<AttributeSetWithValues> _attributeSets = new();

        private void OnValidate()
        {
            foreach (var attributeSet in _attributeSets)
            {
                attributeSet.Update();
            }
        }

        public class AttributeSetAuthoringBaker : Baker<AttributeSetAuthoring>
        {
            public override void Bake(AttributeSetAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                var buffer = AddBuffer<AttributeMap>(entity);
                //count of all attributes in all attribute sets
                var count = authoring._attributeSets.Sum(x => x.AttributeSet.Attributes.Count);
                buffer.InitializeHashMap<AttributeMap, byte, AttributeValue>(count, 5);
                var attributeMap = buffer.AsHashMap<AttributeMap, byte, AttributeValue>();
                foreach (var attributeSet in authoring._attributeSets)
                {
                    foreach (var attribute in attributeSet.AttributeSet.Attributes)
                    {
                        var defaultValue = attributeSet.GetDefaultValue(attribute);
                        attributeMap.Add(attribute.Index, new AttributeValue()
                        {
                            BaseValue = defaultValue,
                            CurrentValue = defaultValue
                        });
                    }
                }
            }
        }
    }
}