using System;
using System.Collections.Generic;
using Battlemage.Attributes.Systems;
using Sirenix.OdinInspector;
using Unity.Entities;
using UnityEngine;
using Waddle.AbilitySystem.Attributes.Authoring;
using Waddle.AbilitySystem.Attributes.Data;

namespace Battlemage.Attributes.Authoring
{
    public class LocalPlayerAttributeBindingAuthoring : MonoBehaviour
    {
        [Serializable, InlineProperty]
        private class AttributeBindingMap
        {
            [HorizontalGroup, HideLabel]
            public WaddleAttribute Attribute;
            [HorizontalGroup, HideLabel]
            public AttributeBindingObject Binding;
        }
        
        [SerializeField] private List<AttributeBindingMap> _attributeBindings;

        public class LocalPlayerAttributeBindingAuthoringBaker : Baker<LocalPlayerAttributeBindingAuthoring>
        {
            public override void Bake(LocalPlayerAttributeBindingAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                var buffer = AddBuffer<PlayerCharacterAttributeBinding>(entity);
                foreach (var attributeBinding in authoring._attributeBindings)
                {
                    if (attributeBinding.Attribute == null || attributeBinding.Binding == null)
                    {
                        continue;
                    }
                    buffer.Add(new PlayerCharacterAttributeBinding
                    {
                        Attribute = attributeBinding.Attribute.Index,
                        Binding = attributeBinding.Binding,
                    });
                }
            }
        }
    }
}