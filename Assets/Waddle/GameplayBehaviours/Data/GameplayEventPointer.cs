using Unity.Entities;

namespace Waddle.GameplayBehaviours.Data
{
    public struct GameplayEventPointer : IBufferElementData
    {
        public long Pointer;
    }
}