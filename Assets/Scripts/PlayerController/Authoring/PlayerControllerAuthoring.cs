using Battlemage.GameplayBehaviours.InputEvents;
using Battlemage.Player;
using Unity.Burst;
using Unity.CharacterController;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Waddle.Runtime.FirstPersonCharacter;
using Waddle.Runtime.GameplayAbilities;
using Waddle.Runtime.GameplayBehaviours;

namespace Battlemage.PlayerController.Authoring
{
    [BurstCompile]
    public class PlayerControllerAuthoring : GameplayBehaviour
    {
        [SerializeField] private GameObject _primaryAbilityPrefab;
        
        public class Baker : Baker<PlayerControllerAuthoring>
        {
            public override void Bake(PlayerControllerAuthoring authoring)
            {
                var entity = GetEntity(authoring, TransformUsageFlags.Dynamic);
                AddComponent(entity, new PlayerController());
                AddComponent(entity, new PlayerAbility()
                {
                    PrimaryAbility = GetEntity(authoring._primaryAbilityPrefab, TransformUsageFlags.Dynamic)
                });
                AddComponent<PlayerCharacterInputs>(entity);
            }
        }
        
        [GameplayEvent(typeof(InputJumpEvent)), BurstCompile]
        private static void OnJump(ref GameplayState state, ref Entity self, ref ButtonState buttonState)
        {
            var character = state.GetComponent<PlayerController>(self).Character;
            var characterCommands = state.GetComponent<FirstPersonCharacterControl>(character);
            characterCommands.Jump = buttonState.WasPressed.IsSet;
            state.SetComponent(character, characterCommands);
        }
        
        [GameplayEvent(typeof(InputLookEvent)), BurstCompile]
        private static void OnLook(ref GameplayState state, ref Entity self, ref float2 value)
        {
            var character = state.GetComponent<PlayerController>(self).Character;
            var characterCommands = state.GetComponent<FirstPersonCharacterControl>(character);
            characterCommands.LookYawPitchDegrees = value;
            state.SetComponent(character, characterCommands);
        }
        
        [GameplayEvent(typeof(InputMoveEvent)), BurstCompile]
        private static void OnMove(ref GameplayState state, ref Entity self, ref float2 value)
        {
            var character = state.GetComponent<PlayerController>(self).Character;
            var characterCommands = state.GetComponent<FirstPersonCharacterControl>(character);
            var characterForward = state.GetForward(character);
            var characterRight = state.GetRight(character);
            var moveVector = (value.y * characterForward) + (value.x * characterRight);
            
            moveVector = MathUtilities.ClampToMaxLength(moveVector, 1f);
            characterCommands.MoveVector = moveVector;
            
            state.SetComponent(character, characterCommands);
        }

        [GameplayEvent(typeof(InputPrimaryAbilityEvent)), BurstCompile]
        private static void OnPrimaryAbility(ref GameplayState state, ref Entity self, ref ButtonState buttonState)
        {
            if (buttonState.WasPressed.IsSet)
            {
                var playerController = state.GetComponent<PlayerController>(self);
                var playerAbility = state.GetComponent<PlayerAbility>(self);
                state.TryActivateAbility(playerController.Character, playerAbility.PrimaryAbility);
            }
        }
    }
}