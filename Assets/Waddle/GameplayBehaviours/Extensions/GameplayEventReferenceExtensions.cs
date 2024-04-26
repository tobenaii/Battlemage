﻿using System;
using System.Collections.Generic;
using Unity.Entities;
using Waddle.GameplayBehaviours.Data;

namespace Waddle.GameplayBehaviours.Extensions
{
    public static class GameplayEventReferenceExtensions
    {
        public static IntPtr GetEventPointer(this DynamicBuffer<GameplayEventReference> buffer, DynamicBuffer<GameplayEventPointer> pointers, GameplayEventHash eventHash)
        {
            return GetEventPointer(buffer, pointers, eventHash.TypeHash, eventHash.MethodHash);
        }
        
        public static IntPtr GetEventPointer(this DynamicBuffer<GameplayEventReference> buffer, DynamicBuffer<GameplayEventPointer> pointers, ulong typeHash, int methodHash = 0)
        {
            return new IntPtr(pointers[GetEventIndex(buffer, typeHash, methodHash)].Pointer);
        }
        
        public static byte GetEventIndex(this DynamicBuffer<GameplayEventReference> buffer, ulong typeHash, int methodHash = 0)
        {
            foreach (var element in buffer)
            {
                if (element.EventHash.TypeHash == typeHash && element.EventHash.MethodHash == methodHash)
                {
                    return element.Index;
                }
            }
            throw new KeyNotFoundException($"Couldn't find event pointer for hash: {typeHash}-{methodHash}");
        }
    }
}