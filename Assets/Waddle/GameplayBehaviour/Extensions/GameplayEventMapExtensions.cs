using System;
using System.Collections.Generic;
using Unity.Entities;
using Waddle.GameplayBehaviour.Data;

namespace Waddle.GameplayBehaviour.Extensions
{
    public static class GameplayEventReferenceExtensions
    {
        public static IntPtr GetEventPointer(this DynamicBuffer<GameplayEventReference> buffer, DynamicBuffer<GameplayEventPointer> pointers, ulong typeHash, int methodHash = 0)
        {
            foreach (var element in buffer)
            {
                if (element.TypeHash == typeHash && element.MethodHash == methodHash)
                {
                    return new IntPtr(pointers[element.Index].Pointer);
                }
            }
            throw new KeyNotFoundException($"Couldn't find event pointer for hash: {typeHash}-{methodHash}");
        }
    }
}