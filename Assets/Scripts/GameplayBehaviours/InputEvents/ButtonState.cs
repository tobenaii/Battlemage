using Unity.NetCode;
using Waddle.Runtime.EntitiesExtended;

namespace Battlemage.GameplayBehaviours.InputEvents
{
    public struct ButtonState
    {
        public InputEvent WasPressed;
        public InputEvent WasReleased;
        public BlittableBool IsDown;

        public void Pressed()
        {
            WasPressed.Set();
            IsDown = true;
        }

        public void Released()
        {
            IsDown = false;
            WasReleased.Set();
        }
    }
}