using Unity.Entities;

namespace Waddle.AbilitySystem.GameplayEffects.Data
{
    public struct GameplayEffect
    {
        public BlobArray<AttributeModifier> AttributeModifiers;
    }

    public struct AttributeModifier
    {
        public enum Operation
        {
            Add,
            Negate,
            Multiply,
            Divide,
            Override,
        }

        public enum ValueType
        {
            Constant,
            Attribute,
        }
        
        public byte ModAttribute;
            
        public Operation OperationType;
            
        public float SourceValue;

        public byte SourceAttribute;
            
        public ValueType SourceValueType;
    }
    
    public interface IGameplayEffectReference
    {
        BlobAssetReference<GameplayEffect> GameplayEffect { get; set; }
    }
    
    public struct OnAbilityActivateGameplayEffect : IBufferElementData, IGameplayEffectReference
    {
        public BlobAssetReference<GameplayEffect> GameplayEffect { get; set; }
    }
    
    public struct OnAbilityHitGameplayEffect : IBufferElementData, IGameplayEffectReference
    {
        public BlobAssetReference<GameplayEffect> GameplayEffect { get; set; }
    }
}