using Unity.Entities;

namespace Waddle.GameplayEffects.Data
{
    public struct GameplayAttributeModifier : IBufferElementData
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
        public float Value;
    }
}