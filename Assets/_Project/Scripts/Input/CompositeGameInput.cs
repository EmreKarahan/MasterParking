using MasterParking.Core;

namespace MasterParking.Input
{
    /// <summary>
    /// Merges touch and (development-only) keyboard input. Touch steering wins
    /// while the on-screen wheel is held; keyboard steering ramps in the core.
    /// </summary>
    public sealed class CompositeGameInput : IGameInput
    {
        private readonly TouchGameInput _touch;
        private readonly KeyboardGameInput _keyboard = new KeyboardGameInput();

        public CompositeGameInput(TouchGameInput touch)
        {
            _touch = touch;
        }

        public Gear Gear => _touch.Gear;

        public void SetGear(Gear gear) => _touch.SetGear(gear);
        public void SetThrottle(bool held) => _touch.SetThrottle(held);
        public void SetBrake(bool held) => _touch.SetBrake(held);
        public void SetSteering(float ratio, bool held) => _touch.SetSteering(ratio, held);
        public void RequestToggleCamera() => _touch.RequestToggleCamera();
        public void RequestPause() => _touch.RequestPause();
        public void RequestRestart() => _touch.RequestRestart();
        public void Reset() => _touch.Reset();

        public GameInputFrame Read()
        {
            GameInputFrame frame = _touch.Read();
            KeyboardGameInput.Snapshot keyboard = _keyboard.Poll();

            if (keyboard.HasGearRequest)
            {
                _touch.SetGear(keyboard.GearRequest);
            }

            frame.Vehicle.Gear = _touch.Gear;
            frame.Vehicle.Throttle |= keyboard.Throttle;
            frame.Vehicle.Brake |= keyboard.Brake;

            if (!frame.Vehicle.SteeringHeld && keyboard.SteeringHeld)
            {
                frame.Vehicle.SteeringHeld = true;
                frame.Vehicle.SteeringTarget = keyboard.SteeringTarget;
                frame.Vehicle.SteeringIsAbsolute = false;
            }

            frame.ToggleCamera |= keyboard.ToggleCamera;
            frame.Pause |= keyboard.Pause;
            frame.Restart |= keyboard.Restart;
            return frame;
        }
    }
}
