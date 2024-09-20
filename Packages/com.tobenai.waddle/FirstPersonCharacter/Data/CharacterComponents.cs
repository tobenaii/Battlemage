using System;
using Unity.CharacterController;
using Unity.Entities;
using Unity.Mathematics;
using Unity.NetCode;

namespace Waddle.FirstPersonCharacter.Data
{
    [Serializable]
    public struct CharacterSettings : IComponentData
    {
        public float GroundMaxSpeed;
        public float GroundedMovementSharpness;
        public float AirAcceleration;
        public float AirMaxSpeed;
        public float AirDrag;
        public float JumpSpeed;
        public float3 Gravity;
        public bool PreventAirAccelerationAgainstUngroundedHits;
        public BasicStepAndSlopeHandlingParameters StepAndSlopeHandling;
    
        public Entity ViewEntity;
        public float MinViewAngle;
        public float MaxViewAngle;
        
        public static CharacterSettings GetDefault()
        {
            return new CharacterSettings
            {
                GroundMaxSpeed = 10f,
                GroundedMovementSharpness = 15f,
                AirAcceleration = 50f,
                AirMaxSpeed = 10f,
                AirDrag = 0f,
                JumpSpeed = 10f,
                Gravity = math.up() * -30f,
                PreventAirAccelerationAgainstUngroundedHits = true,
                StepAndSlopeHandling = BasicStepAndSlopeHandlingParameters.GetDefault(),
                MinViewAngle = -90f,
                MaxViewAngle = 90f,
            };
        }
    }
    
    [Serializable]
    public struct CharacterView : IComponentData
    {
        public Entity CharacterEntity;
    }

    [GhostComponent, Serializable]
    public struct CharacterViewRotation : IComponentData
    {
        [GhostField(Quantization = 1000, Smoothing = SmoothingAction.InterpolateAndExtrapolate)]
        public float CharacterYDegrees;
        
        [GhostField(Quantization = 1000, Smoothing = SmoothingAction.InterpolateAndExtrapolate)]
        public float ViewPitchDegrees;
        
        public quaternion ViewLocalRotation;
    }

    [Serializable]
    public struct FirstPersonCharacterControl : IComponentData
    {
        public float3 MoveVector;
        public float2 LookYawPitchDegrees;
        public bool Jump;
    }
}