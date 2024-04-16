namespace Battlemage.GameplayBehaviours.Data.InputEvents
{
    public struct ButtonState
    {
        public bool WasPressed;
        public bool WasReleased;
        public bool IsDown;

        public void Pressed()
        {
            this.WasPressed = true;
            this.IsDown = true;
        }

        public void Released()
        {
            this.IsDown = false;
            this.WasReleased = true;
        }
    }
}