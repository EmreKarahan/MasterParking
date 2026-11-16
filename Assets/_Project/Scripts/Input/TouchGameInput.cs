using MasterParking.Core;

namespace MasterParking.Input
{
    /// <summary>
    /// Holds the state driven by on-screen controls. uGUI callbacks write here;
    /// gameplay reads one consistent snapshot per frame.
    /// </summary>
    public sealed class TouchGameInput : IGameInput
    {
        private bool _throttle;
        private bool _brake;
        private bool _steeringHeld;
        private float _steering;
        private bool _toggleCamera;
        private bool _pause;
        private bool _restart;

        public Gear Gear { get; private set; } = Gear.Drive;

        public void SetGear(Gear gear) => Gear = gear;
        public void SetThrottle(bool held) => _throttle = held;
        public void SetBrake(bool held) => _brake = held;

        public void SetSteering(float ratio, bool held)
        {
            _steering = MathUtil.Clamp(ratio, -1f, 1f);
            _steeringHeld = held;
        }

        public void RequestToggleCamera() => _toggleCamera = true;
        public void RequestPause() => _pause = true;
        public void RequestRestart() => _restart = true;

        public GameInputFrame Read()
        {
            var frame = new GameInputFrame
            {
                Vehicle = new VehicleInput
                {
                    Throttle = _throttle,
                    Brake = _brake,
                    Gear = Gear,
                    SteeringTarget = _steering,
                    SteeringHeld = _steeringHeld,
                    SteeringIsAbsolute = true
                },
                ToggleCamera = _toggleCamera,
                Pause = _pause,
                Restart = _restart
            };

            _toggleCamera = false;
            _pause = false;
            _restart = false;

            // Held controls intentionally persist: the wheel only raises drag
            // events while the finger moves, and pedals stay down until release.
            return frame;
        }

        public void Reset()
        {
            Gear = Gear.Drive;
            _throttle = false;
            _brake = false;
            _steeringHeld = false;
            _steering = 0f;
            _toggleCamera = false;
            _pause = false;
            _restart = false;
        }
    }
}
