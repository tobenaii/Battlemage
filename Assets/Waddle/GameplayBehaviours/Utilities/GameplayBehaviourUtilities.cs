using System;
using System.Reflection;
using Unity.Entities;
using Waddle.EntitiesExtended.Extensions;
using Waddle.GameplayBehaviours.Data;
using Hash128 = Unity.Entities.Hash128;

namespace Waddle.GameplayBehaviour.Utilities
{
    public static class GameplayBehaviourUtilities
    {
        public static int GetEventPointerIndex(EntityManager entityManager, Type behaviourType,
            MethodInfo method)
        {
            var eventInfos = entityManager.GetSingletonBuffer<GameplayEventInfoElement>();

            for (var i = 0; i < eventInfos.Length; i++)
            {
                var eventInfo = eventInfos[i];
                if (eventInfo.Info.AssemblyQualifiedName.ToString() == behaviourType.AssemblyQualifiedName &&
                    eventInfo.Info.MethodName.ToString() == method.Name)
                {
                    return i;
                }
            }

            return -1;
        }
        
        public static void SetEventPointerByIndex(EntityManager entityManager, int index, IntPtr pointer)
        {
            var eventPointers = entityManager.GetSingletonBuffer<GameplayEventPointer>();
            eventPointers[index] = new GameplayEventPointer { Pointer = pointer.ToInt64() };
        }

        public static Hash128 GetEventHash(Type gameplayBehaviour, ComponentType eventType, MethodInfo method)
        {
            var typeHash = TypeManager.GetTypeInfo(eventType.TypeIndex).StableTypeHash;
            return new Hash128(
                (uint)gameplayBehaviour.GetHashCode(),
                (uint)typeHash.GetHashCode(),
                (uint)GetCleanMethodName(method).GetHashCode(), 0);
        }

        private static string GetCleanMethodName(MethodInfo method)
        {
            return method.Name.Replace("$BurstManaged", "");
        }
    }
}