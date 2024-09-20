using System;
using System.Linq;
using System.Reflection;
using Unity.Burst;
using Unity.Entities;

namespace Waddle.Runtime.GameplayBehaviours
{
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    public partial class GameplayEventSetupSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireForUpdate<InitializeGameplayEvents>();
        }
        
        protected override void OnUpdate()
        {
            var eventSetupDataBuffer = SystemAPI.GetSingletonBuffer<GameplayEventSetupData>();
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var behaviourTypes = assemblies.Select(assembly =>
                assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(GameplayBehaviour)))).SelectMany(x => x);
            
            foreach (var behaviourType in behaviourTypes)
            {
                var eventMethods = behaviourType
                    .GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic)
                    .Where(x => x.GetCustomAttribute<GameplayEventAttribute>() != null);
                foreach (var methodInfo in eventMethods)
                {
                    var componentType = methodInfo.GetCustomAttribute<GameplayEventAttribute>().GameplayEventType;
                    var eventPointer = eventSetupDataBuffer.GetEventPointer(behaviourType, componentType);
                    var eventDelegate = Delegate.CreateDelegate(componentType.GetManagedType().GetCustomAttribute<GameplayEventDefinitionAttribute>().DelegateType, methodInfo);
                    eventPointer.Value.Pointer = BurstCompiler.CompileFunctionPointer(eventDelegate).Value;
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