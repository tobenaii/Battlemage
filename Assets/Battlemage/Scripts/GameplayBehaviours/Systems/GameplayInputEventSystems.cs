using Battlemage.GameplayBehaviours.Data;
using Battlemage.GameplayBehaviours.Data.InputEvents;
using Battlemage.PlayerController.Data;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Waddle.FirstPersonCharacter.Systems;
using Waddle.GameplayBehaviour.Data;
using Waddle.GameplayBehaviour.Extensions;

namespace Battlemage.GameplayBehaviours.Systems
{
    [BurstCompile]
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup), OrderFirst = true)]
    [UpdateBefore(typeof(FirstPersonCharacterVariableUpdateSystem))]
    [UpdateAfter(typeof(BuildCharacterPredictedRotationSystem))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    public partial struct GameplayVariableInputEventSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameplayEventPointer>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var eventPointers = SystemAPI.GetSingletonBuffer<GameplayEventPointer>();
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            
            foreach (var (eventRefs, inputs, entity) in SystemAPI
                         .Query<DynamicBuffer<GameplayEventReference>, RefRO<PlayerCharacterInputs>>()
                         .WithAll<GhostOwnerIsLocal, Simulate>()
                         .WithAny<InputLookEvent, InputPrimaryAbilityEvent>()
                         .WithEntityAccess())
            {
                var source = entity;
                if (SystemAPI.HasComponent<InputLookEvent>(entity))
                {
                    var lookDelta = inputs.ValueRO.LookInputDelta;
                    var lookPointer = eventRefs.GetEventPointer(eventPointers, TypeManager.GetTypeInfo<InputLookEvent>().StableTypeHash);
                    var gameplayState = new GameplayState(state.EntityManager, ecb, SystemAPI.Time);
                    new FunctionPointer<InputLookEvent.Delegate>(lookPointer).Invoke(ref gameplayState, ref source, ref lookDelta);
                }

                if (SystemAPI.HasComponent<InputPrimaryAbilityEvent>(entity))
                {
                    var primaryAbilityInput = inputs.ValueRO.PrimaryAbility;
                    var primaryAbilityPointer = eventRefs.GetEventPointer(eventPointers, TypeManager.GetTypeInfo<InputPrimaryAbilityEvent>().StableTypeHash);
                    var gameplayState = new GameplayState(state.EntityManager, ecb, SystemAPI.Time);
                    new FunctionPointer<InputPrimaryAbilityEvent.Delegate>(primaryAbilityPointer).Invoke(ref gameplayState, ref source, ref primaryAbilityInput);
                }
            }
            
            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }

    [BurstCompile]
    [UpdateInGroup(typeof(PredictedFixedStepSimulationSystemGroup), OrderFirst = true)]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    public partial struct GameplayFixedInputEventSystem : ISystem
    {
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<GameplayEventPointer>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            var gameplayState = new GameplayState(state.EntityManager, ecb, SystemAPI.Time);
            var eventPointers = SystemAPI.GetSingletonBuffer<GameplayEventPointer>();

            foreach (var (eventRefs, inputs, entity) in SystemAPI
                         .Query<DynamicBuffer<GameplayEventReference>, RefRO<PlayerCharacterInputs>>()
                         .WithAll<GhostOwnerIsLocal, Simulate>()
                         .WithAny<InputJumpEvent, InputMoveEvent>()
                         .WithEntityAccess())
            {
                var source = entity;
                if (SystemAPI.HasComponent<InputJumpEvent>(entity))
                {
                    var jumpPointer = eventRefs.GetEventPointer(eventPointers, TypeManager.GetTypeInfo<InputJumpEvent>().StableTypeHash);
                    var jumpInput = inputs.ValueRO.Jump;
                    new FunctionPointer<InputJumpEvent.Delegate>(jumpPointer).Invoke(ref gameplayState, ref source, ref jumpInput);
                }

                if (SystemAPI.HasComponent<InputMoveEvent>(entity))
                {
                    var moveInput = inputs.ValueRO.MoveInput;
                    var movePointer = eventRefs.GetEventPointer(eventPointers, TypeManager.GetTypeInfo<InputMoveEvent>().StableTypeHash);
                    new FunctionPointer<InputMoveEvent.Delegate>(movePointer).Invoke(ref gameplayState, ref source, ref moveInput);
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}