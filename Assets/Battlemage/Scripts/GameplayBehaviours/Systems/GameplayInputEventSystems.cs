using System.Runtime.InteropServices;
using Battlemage.GameplayBehaviours.Data;
using Battlemage.GameplayBehaviours.Data.InputEvents;
using Battlemage.PlayerController.Data;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using UnityEngine;
using Waddle.FirstPersonCharacter.Systems;
using Waddle.GameplayBehaviour.Data;
using Waddle.GameplayBehaviour.Extensions;
using Waddle.GameplayBehaviour.Utilities;
using Hash128 = Unity.Entities.Hash128;

namespace Battlemage.GameplayBehaviours.Systems
{
    [UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
    [UpdateBefore(typeof(FirstPersonCharacterVariableUpdateSystem))]
    [UpdateAfter(typeof(BuildCharacterPredictedRotationSystem))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    public partial class GameplayVariableInputEventSystem : SystemBase
    {
        private static readonly Hash128 LookEventHash = GameplayBehaviourUtilities.GetEventHash(typeof(InputLookEvent));

        protected override void OnCreate()
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }

        protected override void OnUpdate()
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach (var (eventRefs, inputs, entity) in SystemAPI
                         .Query<DynamicBuffer<GameplayEventReference>, RefRO<PlayerCharacterInputs>>()
                         .WithAll<GhostOwnerIsLocal, Simulate>()
                         .WithAny<InputLookEvent>()
                         .WithEntityAccess())
            {
                var source = entity;
                var gameplayState = new GameplayState(EntityManager, ref ecb);

                var lookDelta = inputs.ValueRO.LookInputDelta;
                var pointer = eventRefs.GetEventPointer(LookEventHash);
                Marshal.GetDelegateForFunctionPointer<InputLookEvent.Delegate>(pointer)
                    .Invoke(ref gameplayState, ref source, ref lookDelta);
            }

            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }

    [UpdateInGroup(typeof(PredictedFixedStepSimulationSystemGroup), OrderFirst = true)]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ServerSimulation)]
    public partial class GameplayFixedInputEventSystem : SystemBase
    {
        private static readonly Hash128 JumpEventHash = GameplayBehaviourUtilities.GetEventHash(typeof(InputJumpEvent));
        private static readonly Hash128 MoveEventHash = GameplayBehaviourUtilities.GetEventHash(typeof(InputMoveEvent));

        protected override void OnUpdate()
        {
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach (var (eventRefs, inputs, entity) in SystemAPI
                         .Query<DynamicBuffer<GameplayEventReference>, RefRO<PlayerCharacterInputs>>()
                         .WithAll<GhostOwnerIsLocal, Simulate>()
                         .WithAny<InputJumpEvent, InputMoveEvent>()
                         .WithEntityAccess())
            {
                var source = entity;
                var gameplayState = new GameplayState(EntityManager, ref ecb);

                var jumpPointer = eventRefs.GetEventPointer(JumpEventHash);
                var jumpInput = inputs.ValueRO.JumpState;
                Marshal.GetDelegateForFunctionPointer<InputJumpEvent.Delegate>(jumpPointer)
                    .Invoke(ref gameplayState, ref source, ref jumpInput);

                var moveInput = inputs.ValueRO.MoveInput;
                var movePointer = eventRefs.GetEventPointer(MoveEventHash);
                Marshal.GetDelegateForFunctionPointer<InputMoveEvent.Delegate>(movePointer)
                    .Invoke(ref gameplayState, ref source, ref moveInput);
            }

            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }
}