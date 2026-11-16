using System;

namespace MasterParking.Core
{
    /// <summary>
    /// Ping-pong traffic car movement on the level plane. Pure model so it can be
    /// unit-tested; the Unity view simply mirrors Position/Heading each step.
    /// </summary>
    public sealed class TrafficMoverModel
    {
        private readonly MoverDefinition _definition;
        private float _velocityX;
        private float _velocityZ;

        public TrafficMoverModel(MoverDefinition definition)
        {
            _definition = definition;
            Reset();
        }

        public Vec2 Position { get; private set; }
        public float Heading { get; private set; }
        public float VelocityX => _velocityX;
        public float VelocityZ => _velocityZ;
        public MoverDefinition Definition => _definition;

        public void Reset()
        {
            Position = _definition.Position;
            _velocityX = _definition.DirectionX;
            _velocityZ = _definition.DirectionZ;
            UpdateHeading();
        }

        public void Step(float deltaTime)
        {
            float dt = MathUtil.Clamp(deltaTime, 0f, 0.1f);
            if (dt <= 0f)
            {
                return;
            }

            float x = Position.X + _velocityX * dt;
            float z = Position.Y + _velocityZ * dt;

            if (_velocityX < 0f && x <= _definition.MinX)
            {
                x = _definition.MinX;
                _velocityX = Math.Abs(_velocityX);
            }
            else if (_velocityX > 0f && x >= _definition.MaxX)
            {
                x = _definition.MaxX;
                _velocityX = -Math.Abs(_velocityX);
            }

            if (_velocityZ < 0f && z <= _definition.MinZ)
            {
                z = _definition.MinZ;
                _velocityZ = Math.Abs(_velocityZ);
            }
            else if (_velocityZ > 0f && z >= _definition.MaxZ)
            {
                z = _definition.MaxZ;
                _velocityZ = -Math.Abs(_velocityZ);
            }

            Position = new Vec2(x, z);
            UpdateHeading();
        }

        private void UpdateHeading()
        {
            Heading = Math.Abs(_velocityX) < 1e-5f && Math.Abs(_velocityZ) < 1e-5f
                ? 0f
                : (float)Math.Atan2(_velocityZ, _velocityX);
        }
    }
}
