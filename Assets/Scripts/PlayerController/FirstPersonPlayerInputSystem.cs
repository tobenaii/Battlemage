using Battlemage.Networking;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

namespace Battlemage.PlayerController
{
    [UpdateInGroup(typeof(GhostInputSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class FirstPersonPlayerInputsSystem : SystemBase
    {
        private PlayerInput _playerInput;

        protected override void OnCreate()
        {
            RequireForUpdate(SystemAPI.QueryBuilder().WithAll<PlayerController, PlayerCharacterInputs>().Build());
            RequireForUpdate<NetworkTime>();
            
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            
            _playerInput = new PlayerInput();
            _playerInput.Enable();
            _playerInput.Gameplay.Enable();
        }

        private NetworkTick _prevTick;
        protected override void OnUpdate()
        {
            var gameplayActions = _playerInput.Gameplay;
            foreach (var playerCommands in SystemAPI
                         .Query<RefRW<PlayerCharacterInputs>>()
                         .WithAll<GhostOwnerIsLocal>())
            {
                playerCommands.ValueRW.MoveInput =
                    Vector2.ClampMagnitude(gameplayActions.Move.ReadValue<Vector2>(), 1f);

                float2 mouseLookInputDelta = gameplayActions.Look.ReadValue<Vector2>() * 1.0f;

                NetworkInputUtilities.AddInputDelta(ref playerCommands.ValueRW.LookInputDelta.x, mouseLookInputDelta.x);
                NetworkInputUtilities.AddInputDelta(ref playerCommands.ValueRW.LookInputDelta.y, mouseLookInputDelta.y);

                playerCommands.ValueRW.Jump = default;
                playerCommands.ValueRW.PrimaryAbility = default;
                if (gameplayActions.Jump.WasPressedThisFrame())
                {
                    playerCommands.ValueRW.Jump.Pressed();
                }
                else if (gameplayActions.Jump.WasReleasedThisFrame())
                {
                    playerCommands.ValueRW.Jump.Released();
                }
                
                if (gameplayActions.PrimaryAbility.WasPressedThisFrame())
                {
                    playerCommands.ValueRW.PrimaryAbility.Pressed();
                }
                else if (gameplayActions.PrimaryAbility.WasReleasedThisFrame())
                {
                    playerCommands.ValueRW.PrimaryAbility.Released();
                }
            }
        }
    }
}