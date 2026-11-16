using System;

namespace MasterParking.Core
{
    /// <summary>Small, allocation-free math helpers shared by the simulation.</summary>
    public static class MathUtil
    {
        public const float TwoPi = (float)(Math.PI * 2.0);
        public const float HalfPi = (float)(Math.PI * 0.5);
        public const float Pi = (float)Math.PI;

        public static float Clamp(float value, float min, float max) =>
            value < min ? min : value > max ? max : value;

        public static float Clamp01(float value) => Clamp(value, 0f, 1f);

        public static float MoveTowards(float current, float target, float maxDelta)
        {
            if (Math.Abs(target - current) <= maxDelta)
            {
                return target;
            }

            return current + Math.Sign(target - current) * maxDelta;
        }

        /// <summary>Wraps an angle into the -PI..PI range.</summary>
        public static float WrapAngle(float radians)
        {
            radians %= TwoPi;
            if (radians > Pi)
            {
                radians -= TwoPi;
            }
            else if (radians < -Pi)
            {
                radians += TwoPi;
            }

            return radians;
        }

        /// <summary>Signed shortest rotation from <paramref name="from"/> to <paramref name="to"/>.</summary>
        public static float DeltaAngle(float from, float to) => WrapAngle(to - from);

        /// <summary>
        /// Parking bays are direction-agnostic: a car facing the opposite direction is
        /// still aligned. Returns the absolute error in radians, clamped to 0..PI/2.
        /// </summary>
        public static float PerpendicularHeadingError(float from, float to)
        {
            float error = Math.Abs(WrapAngle(from - to));
            return error > HalfPi ? Math.Abs(Pi - error) : error;
        }

        public static float Lerp(float a, float b, float t) => a + (b - a) * Clamp01(t);

        public static float InverseLerp(float a, float b, float value)
        {
            if (Math.Abs(b - a) < 1e-6f)
            {
                return 0f;
            }

            return Clamp01((value - a) / (b - a));
        }

        public static bool Approximately(float a, float b, float tolerance = 1e-4f) =>
            Math.Abs(a - b) <= tolerance;
    }
}
