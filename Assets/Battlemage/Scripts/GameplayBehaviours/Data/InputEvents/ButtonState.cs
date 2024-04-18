using System.Runtime.InteropServices;

namespace Battlemage.GameplayBehaviours.Data.InputEvents
{
    public struct ButtonState
    {
        [MarshalAs(UnmanagedType.U1)]
        public bool WasPressed;
        [MarshalAs(UnmanagedType.U1)]
        public bool WasReleased;
        [MarshalAs(UnmanagedType.U1)]
        public bool IsDown;

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