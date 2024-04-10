using Battlemage.Attributes.Data;
using Unity.Entities;

namespace Waddle.GameplayEffect.Data
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
        
        public BattlemageAttribute ModAttribute;
        public Operation OperationType;
        public float SourceValue;
    }
}