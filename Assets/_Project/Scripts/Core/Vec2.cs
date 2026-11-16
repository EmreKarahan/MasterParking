using System;

namespace MasterParking.Core
{
    /// <summary>
    /// Engine-independent 2D vector used for the driving plane (world X/Z).
    /// Kept free of UnityEngine so gameplay rules stay unit-testable.
    /// </summary>
    [Serializable]
    public struct Vec2 : IEquatable<Vec2>
    {
        public float X;
        public float Y;

        public Vec2(float x, float y)
        {
            X = x;
            Y = y;
        }

        public static Vec2 Zero => new Vec2(0f, 0f);

        public float SqrMagnitude => X * X + Y * Y;
        public float Magnitude => (float)Math.Sqrt(X * X + Y * Y);

        public Vec2 Normalized
        {
            get
            {
                float magnitude = Magnitude;
                return magnitude > 1e-6f ? new Vec2(X / magnitude, Y / magnitude) : Zero;
            }
        }

        public static Vec2 FromAngle(float radians) =>
            new Vec2((float)Math.Cos(radians), (float)Math.Sin(radians));

        public float ToAngle() => (float)Math.Atan2(Y, X);

        public static float Distance(Vec2 a, Vec2 b)
        {
            float dx = a.X - b.X;
            float dy = a.Y - b.Y;
            return (float)Math.Sqrt(dx * dx + dy * dy);
        }

        public static float SqrDistance(Vec2 a, Vec2 b)
        {
            float dx = a.X - b.X;
            float dy = a.Y - b.Y;
            return dx * dx + dy * dy;
        }

        public static float Dot(Vec2 a, Vec2 b) => a.X * b.X + a.Y * b.Y;

        public static Vec2 Lerp(Vec2 a, Vec2 b, float t)
        {
            t = MathUtil.Clamp01(t);
            return new Vec2(a.X + (b.X - a.X) * t, a.Y + (b.Y - a.Y) * t);
        }

        public static Vec2 operator +(Vec2 a, Vec2 b) => new Vec2(a.X + b.X, a.Y + b.Y);
        public static Vec2 operator -(Vec2 a, Vec2 b) => new Vec2(a.X - b.X, a.Y - b.Y);
        public static Vec2 operator *(Vec2 a, float scalar) => new Vec2(a.X * scalar, a.Y * scalar);

        public bool Equals(Vec2 other) => X.Equals(other.X) && Y.Equals(other.Y);
        public override bool Equals(object obj) => obj is Vec2 other && Equals(other);
        public override int GetHashCode() => (X.GetHashCode() * 397) ^ Y.GetHashCode();
        public override string ToString() => $"({X:0.###}, {Y:0.###})";
    }
}
