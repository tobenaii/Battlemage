using Waddle.Utilities;

namespace Battlemage.GameplayBehaviours.Data.InputEvents
{
    public struct ButtonState
    {
        public BlittableBool WasPressed;
        public BlittableBool WasReleased;
        public BlittableBool IsDown;

        public void Pressed()
        {
            WasPressed = true;
            IsDown = true;
        }

        public void Released()
        {
            IsDown = false;
            WasReleased = true;
        }
    }
}