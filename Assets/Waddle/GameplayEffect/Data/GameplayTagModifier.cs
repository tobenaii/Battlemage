using Unity.Entities;

namespace Waddle.GameplayEffect.Data
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