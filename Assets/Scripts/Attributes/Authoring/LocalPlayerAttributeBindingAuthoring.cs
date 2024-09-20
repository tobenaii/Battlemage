using System;
using System.Collections.Generic;
using Battlemage.Attributes.Data;
using Battlemage.Attributes.Systems;
using Sirenix.OdinInspector;
using Unity.Entities;
using UnityEngine;
using Waddle.GameplayAttributes.Data;

namespace Battlemage.Attributes.Authoring
{
    public class LocalPlayerAttributeBindingAuthoring : MonoBehaviour
    {
        [Serializable, InlineProperty]
        private class AttributeBindingMap
        {
            [HorizontalGroup, HideLabel]
            public BattlemageAttribute Attribute;
            [HorizontalGroup, HideLabel]
            public GameplayAttributeBindingObject Binding;
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
                    if (attributeBinding.Binding == null)
                    {
                        continue;
                    }
                    buffer.Add(new PlayerCharacterAttributeBinding
                    {
                        Attribute = (byte)attributeBinding.Attribute,
                        Binding = attributeBinding.Binding,
                    });
                }
            }
        }
    }
}