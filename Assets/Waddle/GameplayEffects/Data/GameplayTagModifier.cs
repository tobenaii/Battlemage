using Unity.Entities;

namespace Waddle.GameplayEffects.Data
{
    public struct GameplayTagModifier : IBufferElementData
    {
        public enum Operation
        {
            Add,
            Remove
        }
        
        public TypeIndex Tag;
    }
}