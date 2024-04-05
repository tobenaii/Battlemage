using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using Waddle.AbilitySystem.Attributes.Authoring;
using Waddle.AbilitySystem.GameplayEffects.Data;
using Hash128 = Unity.Entities.Hash128;

namespace Waddle.AbilitySystem.GameplayEffects.Authoring
{
    [CreateAssetMenu(menuName = "Waddle/AbilitySystem/GameplayEffect", fileName = "GameplayEffect")]
    public class GameplayEffectAuthoring : ScriptableObject
    {
        [SerializeField] private List<AttributeModifierAuthoring> _attributeModifiers;
        
        public BlobAssetReference<GameplayEffect> Bake(IBaker baker)
        {
            var hash = new Hash128(GetInstanceID().ToString());
            
            if (!baker.TryGetBlobAssetReference<GameplayEffect>(hash, out var blobAssetReference))
            {
                var builder = new BlobBuilder(Allocator.Temp);
                ref var gameplayEffect = ref builder.ConstructRoot<GameplayEffect>();
                var attributeModifiers = builder.Allocate(ref gameplayEffect.AttributeModifiers, _attributeModifiers.Count);
                for (var i = 0; i < _attributeModifiers.Count; i++)
                {
                    ref var attributeModifier = ref attributeModifiers[i];
                    var modifier = _attributeModifiers[i];
                    attributeModifier.ModAttribute = modifier.ModAttribute.Index;
                    attributeModifier.OperationType = modifier.OperationType;
                    attributeModifier.SourceValue = modifier.SourceValue;
                    attributeModifier.SourceAttribute = modifier.SourceValueType != AttributeModifier.ValueType.Attribute ? (byte)0 : modifier.SourceAttribute.Index;
                    attributeModifier.SourceValueType = modifier.SourceValueType;
                }
                blobAssetReference = builder.CreateBlobAssetReference<GameplayEffect>(Allocator.Persistent);
                builder.Dispose();
                baker.AddBlobAssetWithCustomHash(ref blobAssetReference, hash);
            }

            return blobAssetReference;
        }
        
        [Serializable]
        private class AttributeModifierAuthoring
        {
            
            [HideLabel]
            public WaddleAttribute ModAttribute;
            
            [HideLabel]
            public AttributeModifier.Operation OperationType;
            
            [HorizontalGroup, HideLabel]
            [ShowIf(nameof(SourceValueType), AttributeModifier.ValueType.Constant)]
            public float SourceValue;

            [HorizontalGroup, HideLabel]
            [ShowIf(nameof(SourceValueType), AttributeModifier.ValueType.Attribute)]
            public WaddleAttribute SourceAttribute;
            
            [HorizontalGroup(Width = 100), HideLabel]
            public AttributeModifier.ValueType SourceValueType;
        }
    }
}