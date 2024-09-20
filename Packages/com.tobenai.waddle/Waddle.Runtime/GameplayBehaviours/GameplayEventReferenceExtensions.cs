using System;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;

namespace Waddle.Runtime.GameplayBehaviours
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
        
        public static int GetEventPointerBlobIndex(this DynamicBuffer<GameplayEventReference> buffer, ulong eventHash, FixedString32Bytes methodName)
        {
            for (var i = 0; i < buffer.Length; i++)
            {
                var eventReference = buffer[i];
                if (eventReference.EventHash == eventHash && eventReference.MethodName == methodName) return i;
            }
            throw new KeyNotFoundException($"Could not find event pointer for hash: {eventHash} and method name: {methodName}");
        }
    }
}