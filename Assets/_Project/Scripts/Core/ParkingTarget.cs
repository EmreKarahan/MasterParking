using System;

namespace MasterParking.Core
{
    /// <summary>Axis-aligned parking bay definition in level space (metres).</summary>
    [Serializable]
    public sealed class ParkingTarget
    {
        public float X;
        public float Z;
        public float Angle;
        public float Width = 10.5f;
        public float Depth = 5.6f;

        /// <summary>Maximum centre-to-centre distance from the bay centre.</summary>
        public float ToleranceDistance = 2.4f;

        /// <summary>Maximum heading error in radians (0..PI/2).</summary>
        public float ToleranceAngle = 0.22f;

        public Vec2 Center => new Vec2(X, Z);

        public void Validate()
        {
            Width = Math.Max(0.5f, Width);
            Depth = Math.Max(0.5f, Depth);
            ToleranceDistance = Math.Max(0.1f, ToleranceDistance);
            ToleranceAngle = MathUtil.Clamp(ToleranceAngle, 0.01f, MathUtil.HalfPi);
        }
    }
}
