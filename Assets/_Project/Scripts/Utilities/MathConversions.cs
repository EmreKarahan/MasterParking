using MasterParking.Core;
using UnityEngine;

namespace MasterParking.Utilities
{
    /// <summary>Conversions between the pure X/Z plane and Unity's Vector3 space.</summary>
    public static class MathConversions
    {
        public static Vector3 ToWorld(in Vec2 point, float y = 0f) => new Vector3(point.X, y, point.Y);

        public static Vec2 ToVec2(Vector3 point) => new Vec2(point.x, point.z);

        /// <summary>
        /// The car model faces +X. Unity's yaw is clockwise from +Z, so the
        /// authored heading maps to a negative Y rotation.
        /// </summary>
        public static Quaternion HeadingToRotation(float heading) =>
            Quaternion.Euler(0f, -heading * Mathf.Rad2Deg, 0f);

        public static float VectorToHeading(Vector3 direction) =>
            Mathf.Atan2(direction.z, direction.x);

        public static float SnapAngle(float radians, float stepRadians)
        {
            if (stepRadians <= 0f)
            {
                return radians;
            }

            return Mathf.Round(radians / stepRadians) * stepRadians;
        }
    }
}
