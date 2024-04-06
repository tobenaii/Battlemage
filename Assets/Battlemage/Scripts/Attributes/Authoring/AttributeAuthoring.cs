using System;
using System.Collections.Generic;
using Battlemage.Attributes.Data;
using BovineLabs.Core.Iterators;
using Sirenix.OdinInspector;
using Unity.Entities;
using UnityEngine;

namespace Battlemage.Attributes.Authoring
{
    public class AttributeAuthoring : MonoBehaviour
    {
        [SerializeField] 
        private List<AttributeWithValue> _attributes;
        
        [Serializable]
        public class AttributeWithValue
        {
            [HorizontalGroup]
            [SerializeField, HideLabel] private GameplayAttribute _attribute;
            [HorizontalGroup]
            [SerializeField, HideLabel] private float _value;

            public GameplayAttribute Attribute => _attribute;
            public float Value => _value;

            public AttributeWithValue(GameplayAttribute attribute)
            {
                _attribute = attribute;
            }
        }

        public class AttributeSetAuthoringBaker : Baker<AttributeAuthoring>
        {
            public override void Bake(AttributeAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                var buffer = AddBuffer<AttributeMap>(entity);
                var count = authoring._attributes.Count;
                buffer.InitializeHashMap<AttributeMap, byte, AttributeValue>(count, 5);
                var attributeMap = buffer.AsHashMap<AttributeMap, byte, AttributeValue>();
                foreach (var attribute in authoring._attributes)
                {
                    var defaultValue = attribute.Value;
                    attributeMap.TryAdd((byte)attribute.Attribute, new AttributeValue()
                    {
                        BaseValue = defaultValue,
                        CurrentValue = defaultValue
                    });
                }
            }
        }
    }
}