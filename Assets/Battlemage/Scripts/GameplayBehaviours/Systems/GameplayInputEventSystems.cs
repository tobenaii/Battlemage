using Battlemage.GameplayBehaviours.Data.InputEvents;
using Battlemage.Networking.Utilities;
using Battlemage.PlayerController.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using Waddle.GameplayBehaviours.Data;
using Waddle.GameplayBehaviours.Extensions;
using Waddle.GameplayBehaviours.Systems;

namespace Battlemage.GameplayBehaviours.Systems
{
    [UpdateInGroup(typeof(GameplayEventsSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    public partial struct GameplayVariableInputEventSystem : ISystem
    {
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            var gameplayState = new GameplayState(state.EntityManager, ecb, SystemAPI.Time, state.WorldUnmanaged.IsServer());
            
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
                    new FunctionPointer<InputLookEvent.Delegate>(lookPointer).Invoke(gameplayState, source, lookDelta);
                }

                if (SystemAPI.HasComponent<InputPrimaryAbilityEvent>(entity))
                {
                    var primaryAbilityInput = inputs.ValueRO.PrimaryAbility;
                    var primaryAbilityPointer = eventRefs.GetEventPointer(TypeManager.GetTypeInfo<InputPrimaryAbilityEvent>().StableTypeHash);
                    new FunctionPointer<InputPrimaryAbilityEvent.Delegate>(primaryAbilityPointer).Invoke(gameplayState, source, primaryAbilityInput);
                }
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    [UpdateInGroup(typeof(PredictedFixedStepSimulationSystemGroup), OrderFirst = true)]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    public partial struct GameplayFixedInputEventSystem : ISystem
    {
        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            var gameplayState = new GameplayState(state.EntityManager, ecb, SystemAPI.Time, state.WorldUnmanaged.IsServer());

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
                    new FunctionPointer<InputJumpEvent.Delegate>(jumpPointer).Invoke(gameplayState, source, jumpInput);
                }

                if (SystemAPI.HasComponent<InputMoveEvent>(entity))
                {
                    var moveInput = inputs.ValueRO.MoveInput;
                    var movePointer = eventRefs.GetEventPointer(TypeManager.GetTypeInfo<InputMoveEvent>().StableTypeHash);
                    new FunctionPointer<InputMoveEvent.Delegate>(movePointer).Invoke(gameplayState, source, moveInput);
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}