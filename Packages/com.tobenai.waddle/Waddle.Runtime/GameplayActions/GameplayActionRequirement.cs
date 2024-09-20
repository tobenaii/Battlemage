using Unity.Entities;

namespace Waddle.Runtime.GameplayActions
{
    public struct GameplayActionRequirement : IBufferElementData
    {
        public byte Attribute;
        public float Amount;
    }
}