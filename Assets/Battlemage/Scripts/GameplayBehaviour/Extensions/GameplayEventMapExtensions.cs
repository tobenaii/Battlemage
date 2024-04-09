using System;
using Battlemage.GameplayBehaviour.Data;
using Unity.Entities;

namespace Battlemage.GameplayBehaviour.Extensions
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