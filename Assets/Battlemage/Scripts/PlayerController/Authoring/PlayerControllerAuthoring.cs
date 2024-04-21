using AOT;
using Battlemage.GameplayBehaviours.Data;
using Battlemage.GameplayBehaviours.Data.InputEvents;
using Battlemage.PlayerController.Data;
using Unity.Burst;
using Unity.CharacterController;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Scripting;
using Waddle.FirstPersonCharacter.Data;
using Waddle.GameplayBehaviour.Authoring;
using Waddle.GameplayBehaviour.Data;

namespace Battlemage.PlayerController.Authoring
{
    [BurstCompile, Preserve]
    public class PlayerControllerAuthoring : GameplayBehaviourAuthoring
    {
        [GameplayEvent(typeof(InputJumpEvent)), BurstCompile, Preserve, MonoPInvokeCallback(typeof(InputJumpEvent.Delegate))]
        private static void OnJump(ref GameplayState state, ref Entity self, ref ButtonState buttonState)
        {
            var character = state.GetComponent<Data.PlayerController>(self).Character;
            var characterCommands = state.GetComponent<FirstPersonCharacterControl>(character);
            characterCommands.Jump = buttonState.WasPressed;
            state.SetComponent(character, characterCommands);
        }
        
        [GameplayEvent(typeof(InputLookEvent)), BurstCompile, Preserve, MonoPInvokeCallback(typeof(InputLookEvent.Delegate))]
        private static void OnLook(ref GameplayState state, ref Entity self, ref float2 value)
        {
            var character = state.GetComponent<Data.PlayerController>(self).Character;
            var characterCommands = state.GetComponent<FirstPersonCharacterControl>(character);
            characterCommands.LookYawPitchDegrees = value;
            state.SetComponent(character, characterCommands);
        }
        
        [GameplayEvent(typeof(InputMoveEvent)), BurstCompile, Preserve, MonoPInvokeCallback(typeof(InputMoveEvent.Delegate))]
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

        [GameplayEvent(typeof(InputPrimaryAbilityEvent)), BurstCompile, Preserve, MonoPInvokeCallback(typeof(InputPrimaryAbilityEvent.Delegate))]
        private static void OnPrimaryAbility(ref GameplayState state, ref Entity self, ref ButtonState buttonState)
        {
            if (buttonState.WasPressed)
            {
                var character = state.GetComponent<Data.PlayerController>(self).Character;
                state.TryActivateAbility(character, state.GetComponent<Data.PlayerController>(self).PrimaryAbilityPrefab);
            }
        }

        [SerializeField] private GameObject _primaryAbilityPrefab;

        public class Baker : Baker<PlayerControllerAuthoring>
        {
            public override void Bake(PlayerControllerAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.None);
                AddComponent(entity, new Data.PlayerController()
                {
                    PrimaryAbilityPrefab = GetEntity(authoring._primaryAbilityPrefab, TransformUsageFlags.Dynamic)
                });
                AddComponent<PlayerCharacterInputs>(entity);
            }
        }
    }
}