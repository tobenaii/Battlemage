using Unity.Entities;

namespace Waddle.Runtime.GameplayEffects
{
    public struct GameplayTagModifier : IBufferElementData
    {
        public enum Operation
        {
            Add,
            Remove
        }
        
        public TypeIndex Tag;
        public Operation OperationType;
    }
}