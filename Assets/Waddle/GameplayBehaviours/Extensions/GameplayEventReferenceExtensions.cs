using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Waddle.GameplayBehaviours.Data;

namespace Waddle.GameplayBehaviours.Extensions
{
    public static class GameplayEventReferenceExtensions
    {
        public static IntPtr GetEventPointer(this DynamicBuffer<GameplayEventReference> buffer, ulong eventHash)
        {
            foreach (var eventReference in buffer)
            {
                if (eventReference.EventHash == eventHash) return eventReference.Pointer.Value.Pointer;
            }
            throw new KeyNotFoundException("Could not find event pointer for hash: " + eventHash);
        }
        
        public static BlobAssetReference<GameplayEventPointer> GetEventPointerBlob(this DynamicBuffer<GameplayEventReference> buffer, ulong eventHash, FixedString32Bytes methodName)
        {
            foreach (var eventReference in buffer)
            {
                if (eventReference.EventHash == eventHash && eventReference.MethodName == methodName) return eventReference.Pointer;
            }
            throw new KeyNotFoundException($"Could not find event pointer for hash: {eventHash} and method name: {methodName}");
        }
    }
}