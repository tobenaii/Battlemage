using System;
using System.Linq;
using System.Reflection;
using Unity.Entities;
using Waddle.GameplayBehaviours.Data;
using Waddle.GameplayBehaviours.Extensions;

namespace Waddle.GameplayBehaviours.Systems
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    public partial class GameplayEventSetupSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireForUpdate<InitializeGameplayEvents>();
            EntityManager.CreateSingleton(new WaitForSecondsBuffer());
        }
        
        protected override void OnUpdate()
        {
            var eventSetupDataBuffer = SystemAPI.GetSingletonBuffer<GameplayEventSetupData>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var behaviourTypes = assemblies.Select(assembly =>
                assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(Authoring.GameplayBehaviour)))).SelectMany(x => x);
            
            foreach (var behaviourType in behaviourTypes)
            {
                var eventMethods = behaviourType
                    .GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(x => x.GetCustomAttribute<GameplayEventAttribute>() != null);
                foreach (var methodInfo in eventMethods)
                {
                    var componentType = methodInfo.GetCustomAttribute<GameplayEventAttribute>()!.GameplayEventType;
                    var eventPointer = eventSetupDataBuffer.GetEventPointer(behaviourType, componentType);
                    eventPointer.Value.Pointer = methodInfo.MethodHandle.GetFunctionPointer();
                }
            }
#if !UNITY_EDITOR
            EntityManager.DestroyEntity(SystemAPI.GetSingletonEntity<InitializeGameplayEvents>());
#else
            EntityManager.RemoveComponent<InitializeGameplayEvents>(SystemAPI.GetSingletonEntity<InitializeGameplayEvents>());
#endif
        }
    }
}