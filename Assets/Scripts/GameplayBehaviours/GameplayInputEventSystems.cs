using Battlemage.GameplayBehaviours.InputEvents;
using Battlemage.Networking;
using Battlemage.PlayerController;
using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Waddle.Runtime.GameplayBehaviours;
using Waddle.Runtime.GameplayLifecycle;

namespace Battlemage.GameplayBehaviours
{
    [UpdateInGroup(typeof(GameplayEventsSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [BurstCompile]
    public partial struct GameplayVariableInputEventSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var instantiateEcb = SystemAPI.GetSingletonRW<InstantiateCommandBufferSystem.Singleton>().ValueRW
                .CreateCommandBuffer(state.WorldUnmanaged);
            var gameplayState = new GameplayState(state.EntityManager, instantiateEcb, SystemAPI.Time, state.WorldUnmanaged.IsServer());
            
            foreach (var (eventRefs, inputs, inputCommands, entity) in SystemAPI
                         .Query<DynamicBuffer<GameplayEventReference>, RefRO<PlayerCharacterInputs>, DynamicBuffer<InputBufferData<PlayerCharacterInputs>>>()
                         .WithAll<GhostOwnerIsLocal, Simulate>()
                         .WithAny<InputLookEvent, InputPrimaryAbilityEvent>()
                         .WithEntityAccess())
            {
                var source = entity;
                if (SystemAPI.HasComponent<InputLookEvent>(entity))
                {
                    NetworkInputUtilities.GetCurrentAndPreviousTick(SystemAPI.GetSingleton<NetworkTime>(), out var currentTick, out var previousTick);
                    NetworkInputUtilities.GetCurrentAndPreviousTickInputs(inputCommands, currentTick, previousTick, out var currentTickInputs, out var previousTickInputs);
                    float2 lookDelta;
                    lookDelta.x = NetworkInputUtilities.GetInputDelta(currentTickInputs.LookInputDelta.x, previousTickInputs.LookInputDelta.x);
                    lookDelta.y = NetworkInputUtilities.GetInputDelta(currentTickInputs.LookInputDelta.y, previousTickInputs.LookInputDelta.y);
                    var lookPointer = eventRefs.GetEventPointer(TypeManager.GetTypeInfo<InputLookEvent>().StableTypeHash);
                    new FunctionPointer<InputLookEvent.Delegate>(lookPointer).Invoke(ref gameplayState, ref source, ref lookDelta);
                }

                if (SystemAPI.HasComponent<InputPrimaryAbilityEvent>(entity))
                {
                    var primaryAbilityInput = inputs.ValueRO.PrimaryAbility;
                    var primaryAbilityPointer = eventRefs.GetEventPointer(TypeManager.GetTypeInfo<InputPrimaryAbilityEvent>().StableTypeHash);
                    new FunctionPointer<InputPrimaryAbilityEvent.Delegate>(primaryAbilityPointer).Invoke(ref gameplayState, ref source, ref primaryAbilityInput);
                }
            }
        }
    }

    [UpdateInGroup(typeof(PredictedFixedStepSimulationSystemGroup), OrderFirst = true)]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    [BurstCompile]
    public partial struct GameplayFixedInputEventSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var instantiateEcb = SystemAPI.GetSingletonRW<InstantiateCommandBufferSystem.Singleton>().ValueRW
                .CreateCommandBuffer(state.WorldUnmanaged);            
            var gameplayState = new GameplayState(state.EntityManager, instantiateEcb, SystemAPI.Time, state.WorldUnmanaged.IsServer());

            foreach (var (eventRefs, inputs, entity) in SystemAPI
                         .Query<DynamicBuffer<GameplayEventReference>, RefRO<PlayerCharacterInputs>>()
                         .WithAll<GhostOwnerIsLocal, Simulate>()
                         .WithAny<InputJumpEvent, InputMoveEvent>()
                         .WithEntityAccess())
            {
                var source = entity;
                if (SystemAPI.HasComponent<InputJumpEvent>(entity))
                {
                    var jumpPointer = eventRefs.GetEventPointer(TypeManager.GetTypeInfo<InputJumpEvent>().StableTypeHash);
                    var jumpInput = inputs.ValueRO.Jump;
                    new FunctionPointer<InputJumpEvent.Delegate>(jumpPointer).Invoke(ref gameplayState, ref source, ref jumpInput);
                }

                if (SystemAPI.HasComponent<InputMoveEvent>(entity))
                {
                    var moveInput = inputs.ValueRO.MoveInput;
                    var movePointer = eventRefs.GetEventPointer(TypeManager.GetTypeInfo<InputMoveEvent>().StableTypeHash);
                    new FunctionPointer<InputMoveEvent.Delegate>(movePointer).Invoke(ref gameplayState, ref source, ref moveInput);
                }
            }
        }
    }
}