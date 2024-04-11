using System.Runtime.InteropServices;
using Battlemage.GameplayBehaviours.Data;
using Battlemage.GameplayBehaviours.Data.GameplayEvents;
using Battlemage.GameplayBehaviours.Data.InputEvents;
using Battlemage.Networking.Utilities;
using Battlemage.PlayerController.Data;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;
using Waddle.GameplayBehaviour.Data;
using Waddle.GameplayBehaviour.Extensions;
using Waddle.GameplayBehaviour.Utilities;
using Hash128 = Unity.Entities.Hash128;

namespace Battlemage.PlayerController.Systems
{
    [UpdateInGroup(typeof(GhostInputSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class FirstPersonPlayerInputsSystem : SystemBase
    {
        private static readonly Hash128 JumpEventHash = GameplayBehaviourUtilities.GetEventHash(typeof(InputJumpEvent));
        
        private PlayerInput _playerInput;

        protected override void OnCreate()
        {
            RequireForUpdate(SystemAPI.QueryBuilder().WithAll<Data.PlayerController, PlayerCharacterInputs>().Build());
            RequireForUpdate<NetworkTime>();
            
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            
            // Create the input user
            _playerInput = new PlayerInput();
            _playerInput.Enable();
            _playerInput.DefaultMap.Enable();
        }

        protected override void OnUpdate()
        {
            var defaultActionsMap = _playerInput.DefaultMap;
            var ecb = new EntityCommandBuffer(Allocator.Temp);
            foreach (var (playerCommands, eventRefs, entity) in SystemAPI
                         .Query<RefRW<PlayerCharacterInputs>, DynamicBuffer<GameplayEventReference>>()
                         .WithAll<GhostOwnerIsLocal>()
                         .WithEntityAccess())
            {
                playerCommands.ValueRW.MoveInput =
                    Vector2.ClampMagnitude(defaultActionsMap.Move.ReadValue<Vector2>(), 1f);

                float2 mouseLookInputDelta = defaultActionsMap.LookDelta.ReadValue<Vector2>() * 1.0f;
                NetworkInputUtilities.AddInputDelta(ref playerCommands.ValueRW.LookInputDelta.x,
                    mouseLookInputDelta.x);
                NetworkInputUtilities.AddInputDelta(ref playerCommands.ValueRW.LookInputDelta.y,
                    mouseLookInputDelta.y);

                // Jump
                playerCommands.ValueRW.JumpPressed = default;
                if (defaultActionsMap.Jump.WasPressedThisFrame())
                {
                    playerCommands.ValueRW.JumpPressed.Set();
                    var gameplayState = new GameplayState(EntityManager, ref ecb);
                    var pointer = eventRefs.GetEventPointer(JumpEventHash);
                    var source = entity;
                    var buttonState = new ButtonState()
                    {
                        WasPressed = true
                    };
                    Marshal.GetDelegateForFunctionPointer<InputJumpEvent.Delegate>(pointer).Invoke(ref gameplayState, ref source, ref buttonState);
                }
            }
            ecb.Playback(EntityManager);
            ecb.Dispose();
        }
    }
}