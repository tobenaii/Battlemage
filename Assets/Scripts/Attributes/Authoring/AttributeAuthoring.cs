using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.Entities;
using UnityEngine;
using Waddle.Runtime.GameplayAttributes;

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
                var buffer = AddBuffer<GameplayAttribute>(entity);
                foreach (var attribute in authoring._attributes)
                {
                    var defaultValue = attribute.Value;
                    buffer.Add(new GameplayAttribute()
                    {
                        BaseValue = defaultValue,
                        CurrentValue = defaultValue
                    });
                }
            }
        }
    }
}