using System;
using System.Collections.Generic;
using Battlemage.Attributes.Data;
using BovineLabs.Core.Iterators;
using Sirenix.OdinInspector;
using Unity.Entities;
using UnityEngine;
using Waddle.GameplayAttributes.Data;

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
            [SerializeField, HideLabel] private BattlemageAttribute _attribute;
            [HorizontalGroup]
            [SerializeField, HideLabel] private float _value;

            public BattlemageAttribute Attribute => _attribute;
            public float Value => _value;

            public AttributeWithValue(BattlemageAttribute attribute)
            {
                _attribute = attribute;
            }
        }

        public class AttributeSetAuthoringBaker : Baker<AttributeAuthoring>
        {
            public override void Bake(AttributeAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                var buffer = AddBuffer<GameplayAttributeMap>(entity);
                var count = authoring._attributes.Count;
                buffer.InitializeHashMap<GameplayAttributeMap, byte, GameplayAttribute>(count, 5);
                var attributeMap = buffer.AsHashMap<GameplayAttributeMap, byte, GameplayAttribute>();
                foreach (var attribute in authoring._attributes)
                {
                    var defaultValue = attribute.Value;
                    attributeMap.TryAdd((byte)attribute.Attribute, new GameplayAttribute()
                    {
                        BaseValue = defaultValue,
                        CurrentValue = defaultValue
                    });
                }
            }
        }
    }
}