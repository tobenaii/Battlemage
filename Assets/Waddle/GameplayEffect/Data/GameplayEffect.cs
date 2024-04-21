using Unity.Entities;

namespace Waddle.GameplayEffect.Data
{
    public struct GameplayEffect : IBufferElementData
    {
        public int AttributeModifiers;
        public int GameplayTags;
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
        
        public byte ModAttribute;
        public Operation OperationType;
        public float SourceValue;
    }
}