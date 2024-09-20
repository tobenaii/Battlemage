using Unity.Entities;

namespace Waddle.GameplayActions.Data
{
    public struct GameplayActionRequirement : IBufferElementData
    {
        public byte Attribute;
        public float Amount;
    }
}