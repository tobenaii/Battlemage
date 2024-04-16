using Battlemage.GameplayBehaviours.Data;
using Battlemage.GameplayBehaviours.Data.InputEvents;
using Battlemage.PlayerController.Data;
using Unity.CharacterController;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Waddle.FirstPersonCharacter.Data;
using Waddle.GameplayBehaviour.Authoring;
using Waddle.GameplayBehaviour.Data;

namespace Battlemage.PlayerController.Authoring
{
    [DisallowMultipleComponent]
    public class PlayerControllerAuthoring : GameplayBehaviourAuthoring
    {
        [GameplayEvent(typeof(InputJumpEvent))]
        private static void OnJump(ref GameplayState state, ref Entity self, ref ButtonState buttonState)
        {
            var character = state.GetComponent<Data.PlayerController>(self).Character;
            var characterCommands = state.GetComponent<FirstPersonCharacterControl>(character);
            characterCommands.Jump = buttonState.WasPressed;
            state.SetComponent(character, characterCommands);
        }
        
        [GameplayEvent(typeof(InputLookEvent))]
        private static void OnLook(ref GameplayState state, ref Entity self, ref float2 value)
        {
            var character = state.GetComponent<Data.PlayerController>(self).Character;
            var characterCommands = state.GetComponent<FirstPersonCharacterControl>(character);
            characterCommands.LookYawPitchDegrees = value;
            state.SetComponent(character, characterCommands);
        }
        
        [GameplayEvent(typeof(InputMoveEvent))]
        private static void OnMove(ref GameplayState state, ref Entity self, ref float2 value)
        {
            var character = state.GetComponent<Data.PlayerController>(self).Character;
            var characterCommands = state.GetComponent<FirstPersonCharacterControl>(character);
            var characterForward = state.GetForward(character);
            var characterRight = state.GetRight(character);
            var moveVector = (value.y * characterForward) + (value.x * characterRight);
            moveVector = MathUtilities.ClampToMaxLength(moveVector, 1f);
            characterCommands.MoveVector = moveVector;
            state.SetComponent(character, characterCommands);
        }

        public class Baker : Baker<PlayerControllerAuthoring>
        {
            public override void Bake(PlayerControllerAuthoring authoring)
            {
                Entity entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new Data.PlayerController());
                AddComponent<PlayerCharacterInputs>(entity);
            }
        }
    }
}