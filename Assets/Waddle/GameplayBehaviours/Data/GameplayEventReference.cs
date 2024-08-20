﻿using Unity.Collections;
using Unity.Entities;

namespace Waddle.GameplayBehaviours.Data
{
    [InternalBufferCapacity(0)]
    public struct GameplayEventReference : IBufferElementData
    {
        public ulong EventHash;
        public FixedString32Bytes MethodName;
        public BlobAssetReference<GameplayEventPointer> Pointer;
    }
}