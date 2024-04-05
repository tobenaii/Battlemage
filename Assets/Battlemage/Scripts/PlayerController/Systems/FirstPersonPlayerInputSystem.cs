using Battlemage.Networking.Utilities;
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

            // Create the input user
            _playerInput = new PlayerInput();
            _playerInput.Enable();
            _playerInput.DefaultMap.Enable();
        }

        protected override void OnUpdate()
        {
            var defaultActionsMap = _playerInput.DefaultMap;

            foreach (var playerCommands in SystemAPI
                         .Query<RefRW<PlayerCharacterInputs>>()
                         .WithAll<GhostOwnerIsLocal>())
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
                }
            }
        }
    }
}