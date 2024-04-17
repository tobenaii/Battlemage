using Battlemage.PlayerController.Data;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;
using UnityEngine;

namespace Battlemage.PlayerController.Systems
{
    [UpdateInGroup(typeof(GhostInputSystemGroup))]
    [WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation)]
    public partial class FirstPersonPlayerInputsSystem : SystemBase
    {
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
            RequireForUpdate<NetworkTime>();
        }

        private NetworkTick _prevTick;
        protected override void OnUpdate()
        {
            var networkTime = SystemAPI.GetSingleton<NetworkTime>();
            var currentTick = networkTime.ServerTick;
            var defaultActionsMap = _playerInput.DefaultMap;
            foreach (var playerCommands in SystemAPI
                         .Query<RefRW<PlayerCharacterInputs>>()
                         .WithAll<GhostOwnerIsLocal>())
            {
                playerCommands.ValueRW.MoveInput =
                    Vector2.ClampMagnitude(defaultActionsMap.Move.ReadValue<Vector2>(), 1f);

                float2 mouseLookInputDelta = defaultActionsMap.LookDelta.ReadValue<Vector2>() * 1.0f;
                if (_prevTick != currentTick)
                {
                    playerCommands.ValueRW.LookInputDelta.x = 0;
                    playerCommands.ValueRW.LookInputDelta.y = 0;
                    _prevTick = currentTick;
                    playerCommands.ValueRW.JumpState = default;
                }

                playerCommands.ValueRW.LookInputDelta.x += mouseLookInputDelta.x;
                playerCommands.ValueRW.LookInputDelta.y += mouseLookInputDelta.y;

                if (defaultActionsMap.Jump.WasPressedThisFrame())
                {
                    playerCommands.ValueRW.JumpState.Pressed();
                }
                else if (defaultActionsMap.Jump.WasReleasedThisFrame())
                {
                    playerCommands.ValueRW.JumpState.Released();
                }
            }
        }
    }
}