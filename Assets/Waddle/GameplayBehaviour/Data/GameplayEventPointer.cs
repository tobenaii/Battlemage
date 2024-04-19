using System;
using Unity.Entities;

namespace Waddle.GameplayBehaviour.Data
{
    public struct GameplayEventPointer : IBufferElementData
    {
        public long Pointer;
    }
}