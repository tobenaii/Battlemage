using Battlemage.Attributes.Data;
using Unity.Entities;

namespace Battlemage.GameplayEffects.Data
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
        
        public GameplayAttribute ModAttribute;
        public Operation OperationType;
        public float SourceValue;
    }
}