using System;
using Unity.Entities;

namespace Waddle.Runtime.GameplayBehaviours
{
    public static class EventSetupDataBufferExtensions
    {
        public static BlobAssetReference<GameplayEventPointer> GetEventPointer(this DynamicBuffer<GameplayEventSetupData> eventSetupDataBuffer, Type behaviourType, ComponentType componentType)
        {
            foreach (var eventSetupData in eventSetupDataBuffer)
            {
                if (eventSetupData.EventHash == TypeManager.GetTypeInfo(componentType.TypeIndex).StableTypeHash
                    && eventSetupData.GameplayBehaviourHash == behaviourType.FullName!.GetHashCode())
                {
                    return eventSetupData.Pointer;
                }
            }
            throw new InvalidOperationException($"Could not find event pointer for {behaviourType.FullName} and {componentType.GetManagedType()}");
        }
    }
}