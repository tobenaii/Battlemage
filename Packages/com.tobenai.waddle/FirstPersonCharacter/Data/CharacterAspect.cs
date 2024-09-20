using Unity.CharacterController;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Waddle.FirstPersonCharacter.Utilities;

namespace Waddle.FirstPersonCharacter.Data
{
    public struct FirstPersonCharacterUpdateContext
    {
        public void OnSystemCreate(ref SystemState state)
        {
        }

        public void OnSystemUpdate(ref SystemState state)
        {
        }
    }

    public readonly partial struct CharacterAspect : IAspect, IKinematicCharacterProcessor<FirstPersonCharacterUpdateContext>
    {
        private readonly KinematicCharacterAspect _kinematicCharacterAspect;
        private readonly RefRO<CharacterSettings> _characterSettings;
        private readonly RefRW<CharacterViewRotation> _characterViewRotation;
        private readonly RefRW<FirstPersonCharacterControl> _characterControl;

        public void PhysicsUpdate(ref FirstPersonCharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext)
        {
            var characterComponent = _characterSettings.ValueRO;
            ref var characterBody = ref _kinematicCharacterAspect.CharacterBody.ValueRW;
            ref var characterPosition = ref _kinematicCharacterAspect.LocalTransform.ValueRW.Position;

            // First phase of default character update
            _kinematicCharacterAspect.Update_Initialize(in this, ref context, ref baseContext, ref characterBody, baseContext.Time.DeltaTime);
            _kinematicCharacterAspect.Update_ParentMovement(in this, ref context, ref baseContext, ref characterBody, ref characterPosition, characterBody.WasGroundedBeforeCharacterUpdate);
            _kinematicCharacterAspect.Update_Grounding(in this, ref context, ref baseContext, ref characterBody, ref characterPosition);
        
            // Update desired character velocity after grounding was detected, but before doing additional processing that depends on velocity
            HandleVelocityControl(ref context, ref baseContext);

            // Second phase of default character update
            _kinematicCharacterAspect.Update_PreventGroundingFromFutureSlopeChange(in this, ref context, ref baseContext, ref characterBody, in characterComponent.StepAndSlopeHandling);
            _kinematicCharacterAspect.Update_GroundPushing(in this, ref context, ref baseContext, characterComponent.Gravity);
            _kinematicCharacterAspect.Update_MovementAndDecollisions(in this, ref context, ref baseContext, ref characterBody, ref characterPosition);
            _kinematicCharacterAspect.Update_MovingPlatformDetection(ref baseContext, ref characterBody); 
            _kinematicCharacterAspect.Update_ParentMomentum(ref baseContext, ref characterBody);
            _kinematicCharacterAspect.Update_ProcessStatefulCharacterHits();
        }

        private void HandleVelocityControl(ref FirstPersonCharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext)
        { 
            float deltaTime = baseContext.Time.DeltaTime;
            var characterComponent = _characterSettings.ValueRO;

            ref var characterBody = ref _kinematicCharacterAspect.CharacterBody.ValueRW;
            ref var characterControl = ref _characterControl.ValueRW;

            // Rotate move input and velocity to take into account parent rotation
            if(characterBody.ParentEntity != Entity.Null)
            {
                characterControl.MoveVector = math.rotate(characterBody.RotationFromParent, characterControl.MoveVector);
                characterBody.RelativeVelocity = math.rotate(characterBody.RotationFromParent, characterBody.RelativeVelocity);
            }
        
            if (characterBody.IsGrounded)
            {
                // Move on ground
                var targetVelocity = characterControl.MoveVector * characterComponent.GroundMaxSpeed;
                CharacterControlUtilities.StandardGroundMove_Interpolated(ref characterBody.RelativeVelocity, targetVelocity, characterComponent.GroundedMovementSharpness, deltaTime, characterBody.GroundingUp, characterBody.GroundHit.Normal);

                // Jump
                if (characterControl.Jump)
                {
                    CharacterControlUtilities.StandardJump(ref characterBody, characterBody.GroundingUp * characterComponent.JumpSpeed, true, characterBody.GroundingUp);
                }
            }
            else
            {
                // Move in air
                var airAcceleration = characterControl.MoveVector * characterComponent.AirAcceleration;
                if (math.lengthsq(airAcceleration) > 0f)
                {
                    var tmpVelocity = characterBody.RelativeVelocity;
                    CharacterControlUtilities.StandardAirMove(ref characterBody.RelativeVelocity, airAcceleration, characterComponent.AirMaxSpeed, characterBody.GroundingUp, deltaTime, false);

                    // Cancel air acceleration from input if we would hit a non-grounded surface (prevents air-climbing slopes at high air accelerations)
                    if (characterComponent.PreventAirAccelerationAgainstUngroundedHits && _kinematicCharacterAspect.MovementWouldHitNonGroundedObstruction(in this, ref context, ref baseContext, characterBody.RelativeVelocity * deltaTime, out _))
                    {
                        characterBody.RelativeVelocity = tmpVelocity;
                    }
                }
            
                // Gravity
                CharacterControlUtilities.AccelerateVelocity(ref characterBody.RelativeVelocity, characterComponent.Gravity, deltaTime);

                // Drag
                CharacterControlUtilities.ApplyDragToVelocity(ref characterBody.RelativeVelocity, deltaTime, characterComponent.AirDrag);
            }
        }

        public void VariableUpdate(ref FirstPersonCharacterUpdateContext context, ref KinematicCharacterUpdateContext baseContext)
        {
            var characterComponent = _characterSettings.ValueRO;
            var characterControl = _characterControl.ValueRO;
            
            ref var characterBody = ref _kinematicCharacterAspect.CharacterBody.ValueRW;
            ref var characterViewRotation = ref _characterViewRotation.ValueRW;
            ref var characterRotation = ref _kinematicCharacterAspect.LocalTransform.ValueRW.Rotation;

            // Add rotation from parent body to the character rotation
            // (this is for allowing a rotating moving platform to rotate your character as well, and handle interpolation properly)
            KinematicCharacterUtilities.AddVariableRateRotationFromFixedRateRotation(ref characterRotation, characterBody.RotationFromParent, baseContext.Time.DeltaTime, characterBody.LastPhysicsUpdateDeltaTime);
        
            // Compute character & view rotations from rotation input
            CharacterUtilities.ComputeFinalRotationsFromRotationDelta(
                ref characterViewRotation.ViewPitchDegrees,
                ref characterViewRotation.CharacterYDegrees,
                math.up(),
                characterControl.LookYawPitchDegrees,
                0, // don't include roll angle in simulation
                characterComponent.MinViewAngle, 
                characterComponent.MaxViewAngle,
                out characterRotation,
                out characterViewRotation.ViewLocalRotation);
        }
    
        #region Character Processor Callbacks
        public void UpdateGroundingUp(
            ref FirstPersonCharacterUpdateContext context,
            ref KinematicCharacterUpdateContext baseContext)
        {
            ref var characterBody = ref _kinematicCharacterAspect.CharacterBody.ValueRW;
        
            _kinematicCharacterAspect.Default_UpdateGroundingUp(ref characterBody);
        }
    
        public bool CanCollideWithHit(
            ref FirstPersonCharacterUpdateContext context, 
            ref KinematicCharacterUpdateContext baseContext,
            in BasicHit hit)
        {
            return PhysicsUtilities.IsCollidable(hit.Material);
        }

        public bool IsGroundedOnHit(
            ref FirstPersonCharacterUpdateContext context, 
            ref KinematicCharacterUpdateContext baseContext,
            in BasicHit hit, 
            int groundingEvaluationType)
        {
            var characterComponent = _characterSettings.ValueRO;
        
            return _kinematicCharacterAspect.Default_IsGroundedOnHit(
                in this,
                ref context,
                ref baseContext,
                in hit,
                in characterComponent.StepAndSlopeHandling,
                groundingEvaluationType);
        }

        public void OnMovementHit(
            ref FirstPersonCharacterUpdateContext context,
            ref KinematicCharacterUpdateContext baseContext,
            ref KinematicCharacterHit hit,
            ref float3 remainingMovementDirection,
            ref float remainingMovementLength,
            float3 originalVelocityDirection,
            float hitDistance)
        {
            ref var characterBody = ref _kinematicCharacterAspect.CharacterBody.ValueRW;
            ref var characterPosition = ref _kinematicCharacterAspect.LocalTransform.ValueRW.Position;
            CharacterSettings characterComponent = _characterSettings.ValueRO;
        
            _kinematicCharacterAspect.Default_OnMovementHit(
                in this,
                ref context,
                ref baseContext,
                ref characterBody,
                ref characterPosition,
                ref hit,
                ref remainingMovementDirection,
                ref remainingMovementLength,
                originalVelocityDirection,
                hitDistance,
                characterComponent.StepAndSlopeHandling.StepHandling,
                characterComponent.StepAndSlopeHandling.MaxStepHeight,
                characterComponent.StepAndSlopeHandling.CharacterWidthForStepGroundingCheck);
        }

        public void OverrideDynamicHitMasses(
            ref FirstPersonCharacterUpdateContext context,
            ref KinematicCharacterUpdateContext baseContext,
            ref PhysicsMass characterMass,
            ref PhysicsMass otherMass,
            BasicHit hit)
        {
        }

        public void ProjectVelocityOnHits(
            ref FirstPersonCharacterUpdateContext context,
            ref KinematicCharacterUpdateContext baseContext,
            ref float3 velocity,
            ref bool characterIsGrounded,
            ref BasicHit characterGroundHit,
            in DynamicBuffer<KinematicVelocityProjectionHit> velocityProjectionHits,
            float3 originalVelocityDirection)
        {
            var characterComponent = _characterSettings.ValueRO;
        
            _kinematicCharacterAspect.Default_ProjectVelocityOnHits(
                ref velocity,
                ref characterIsGrounded,
                ref characterGroundHit,
                in velocityProjectionHits,
                originalVelocityDirection,
                characterComponent.StepAndSlopeHandling.ConstrainVelocityToGroundPlane);
        }
        #endregion
    }
}