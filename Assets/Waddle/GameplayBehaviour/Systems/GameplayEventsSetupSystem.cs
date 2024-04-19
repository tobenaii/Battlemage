using System;
using System.Reflection;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;
using Waddle.GameplayBehaviour.Data;

namespace Waddle.GameplayBehaviour.Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup), OrderFirst = true)]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    public partial class GameplayEventsSetupSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireForUpdate<InitializeGameplayEvents>();
        }
        
        protected override void OnUpdate()
        {
            var eventInfos = SystemAPI.GetSingletonBuffer<GameplayEventInfoElement>();
            var eventPointers = SystemAPI.GetSingletonBuffer<GameplayEventPointer>();
            foreach (var eventInfo in eventInfos)
            {
                var blobMapping = eventInfo.Info;
                var behaviourType = Type.GetType(blobMapping.AssemblyQualifiedName.ToString())!;
                var methodInfo = behaviourType.GetMethod(blobMapping.MethodName.ToString(), BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)!;
                var attribute = methodInfo.GetCustomAttribute<GameplayEventAttribute>();
                var delegateType = attribute.GameplayEventType.GetManagedType().GetCustomAttribute<GameplayEventDefinitionAttribute>().DelegateType;
                
                var eventDelegate = Delegate.CreateDelegate(delegateType, methodInfo);
                var newPointer = BurstCompiler.CompileFunctionPointer(eventDelegate).Value;

                eventPointers.Add(new GameplayEventPointer() { Pointer = newPointer.ToInt64() });
            }
            
            EntityManager.RemoveComponent<InitializeGameplayEvents>(SystemAPI.GetSingletonEntity<InitializeGameplayEvents>());
#if !UNITY_EDITOR
            EntityManager.RemoveComponent<GameplayEventInfoElement>(SystemAPI.GetSingletonEntity<GameplayEventInfoElement>());
#endif
            Debug.Log("Crash Check Events End");
        }
    }
}