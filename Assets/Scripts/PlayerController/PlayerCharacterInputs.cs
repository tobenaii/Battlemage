using System;
using Battlemage.GameplayBehaviours.InputEvents;
using Unity.Mathematics;
using Unity.NetCode;

namespace Battlemage.PlayerController
{
    [Serializable]
    public struct PlayerCharacterInputs : IInputComponentData
    {
        public float2 MoveInput;
        public float2 LookInputDelta;
        public ButtonState Jump;
        public ButtonState PrimaryAbility;
    }
}