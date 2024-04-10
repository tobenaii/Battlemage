using System;
using Unity.Entities;
using Waddle.GameplayBehaviour.Data;

namespace Waddle.GameplayBehaviour.Extensions
{
    public static class GameplayEventReferenceExtensions
    {
        public static IntPtr GetEventPointer(this DynamicBuffer<GameplayEventReference> buffer, Hash128 hash)
        {
            foreach (var element in buffer)
            {
                if (element.Hash == hash)
                {
                    return element.EventPointerRef.Value.Pointer;
                }
            }
            return IntPtr.Zero;
        }
    }
}