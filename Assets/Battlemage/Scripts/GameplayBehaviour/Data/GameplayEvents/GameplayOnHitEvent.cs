﻿using System;
using Unity.Entities;

namespace Battlemage.GameplayBehaviour.Data.GameplayEvents
{
    [GameplayEventDefinition]
    public struct GameplayOnHitEvent : IGameplayEvent
    {
        public BlobAssetReference<EventPointer> EventPointerRef { get; set; }
    }
}