using Unity.CharacterController;
using Unity.Mathematics;

namespace Battlemage.Movement.Utilities
{
    public static class CharacterUtilities
    {
        public static void ComputeFinalRotationsFromRotationDelta(
            ref float viewPitchDegrees,
            ref float characterRotationYDegrees,
            float3 characterTransformUp,
            float2 yawPitchDeltaDegrees,
            float viewRollDegrees,
            float minPitchDegrees,
            float maxPitchDegrees,
            out quaternion characterRotation,
            out quaternion viewLocalRotation)
        {
            // Yaw
            characterRotationYDegrees += yawPitchDeltaDegrees.x;
            ComputeRotationFromYAngleAndUp(characterRotationYDegrees, characterTransformUp, out characterRotation);

            // Pitch
            viewPitchDegrees += yawPitchDeltaDegrees.y;
            viewPitchDegrees = math.clamp(viewPitchDegrees, minPitchDegrees, maxPitchDegrees);
            viewLocalRotation = CalculateLocalViewRotation(viewPitchDegrees, viewRollDegrees);
        }

        public static void ComputeRotationFromYAngleAndUp(
            float characterRotationYDegrees,
            float3 characterTransformUp,
            out quaternion characterRotation)
        {
            characterRotation = math.mul(MathUtilities.CreateRotationWithUpPriority(characterTransformUp, math.forward()), quaternion.Euler(0f, math.radians(characterRotationYDegrees), 0f));
        }

        public static quaternion CalculateLocalViewRotation(float viewPitchDegrees, float viewRollDegrees)
        {
            // Pitch
            quaternion viewLocalRotation = quaternion.AxisAngle(-math.right(), math.radians(viewPitchDegrees));

            // Roll
            viewLocalRotation = math.mul(viewLocalRotation, quaternion.AxisAngle(math.forward(), math.radians(viewRollDegrees)));

            return viewLocalRotation;
        }
    }
}
