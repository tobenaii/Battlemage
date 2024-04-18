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
using Waddle.GameplayBehaviour.Utilities;
using Hash128 = Unity.Entities.Hash128;

namespace Battlemage.GameplayBehaviours.Systems
{
    [BurstCompile]
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup), OrderFirst = true)]
    [UpdateBefore(typeof(FirstPersonCharacterVariableUpdateSystem))]
    [UpdateAfter(typeof(BuildCharacterPredictedRotationSystem))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    public partial struct GameplayVariableInputEventSystem : ISystem
    {
        private static readonly Hash128 LookEventHash = GameplayBehaviourUtilities.GetEventHash(ComponentType.ReadWrite<InputLookEvent>());
        private static readonly Hash128 PrimaryAbilityEventHash = GameplayBehaviourUtilities.GetEventHash(ComponentType.ReadWrite<InputPrimaryAbilityEvent>());

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (eventRefs, inputs, entity) in SystemAPI
                         .Query<DynamicBuffer<GameplayEventReference>, RefRO<PlayerCharacterInputs>>()
                         .WithAll<GhostOwnerIsLocal, Simulate>()
                         .WithAny<InputJumpEvent, InputMoveEvent>()
                         .WithEntityAccess())
            {
                var source = entity;
                var gameplayState = new GameplayState(state.EntityManager, ref ecb);
                if (SystemAPI.HasComponent<InputLookEvent>(entity))
                {
                    var lookDelta = inputs.ValueRO.LookInputDelta;
                    var lookPointer = eventRefs.GetEventPointer(LookEventHash);
                    new FunctionPointer<InputLookEvent.Delegate>(lookPointer).Invoke(ref gameplayState, ref source, ref lookDelta);
                }

                if (SystemAPI.HasComponent<InputPrimaryAbilityEvent>(entity))
                {
                    var primaryAbilityInput = inputs.ValueRO.PrimaryAbility;
                    var primaryAbilityPointer = eventRefs.GetEventPointer(PrimaryAbilityEventHash);
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
        private static readonly Hash128 JumpEventHash = GameplayBehaviourUtilities.GetEventHash(ComponentType.ReadWrite<InputJumpEvent>());
        private static readonly Hash128 MoveEventHash = GameplayBehaviourUtilities.GetEventHash(ComponentType.ReadWrite<InputMoveEvent>());

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);

            foreach (var (eventRefs, inputs, entity) in SystemAPI
                         .Query<DynamicBuffer<GameplayEventReference>, RefRO<PlayerCharacterInputs>>()
                         .WithAll<GhostOwnerIsLocal, Simulate>()
                         .WithAny<InputLookEvent, InputPrimaryAbilityEvent>()
                         .WithEntityAccess())
            {
                var source = entity;
                var gameplayState = new GameplayState(state.EntityManager, ref ecb);
                if (SystemAPI.HasComponent<InputJumpEvent>(entity))
                {
                    var jumpPointer = eventRefs.GetEventPointer(JumpEventHash);
                    var jumpInput = inputs.ValueRO.Jump;
                    new FunctionPointer<InputJumpEvent.Delegate>(jumpPointer).Invoke(ref gameplayState, ref source, ref jumpInput);
                }

                if (SystemAPI.HasComponent<InputMoveEvent>(entity))
                {
                    var moveInput = inputs.ValueRO.MoveInput;
                    var movePointer = eventRefs.GetEventPointer(MoveEventHash);
                    new FunctionPointer<InputMoveEvent.Delegate>(movePointer).Invoke(ref gameplayState, ref source, ref moveInput);
                }
            }

            ecb.Playback(state.EntityManager);
            ecb.Dispose();
        }
    }
}