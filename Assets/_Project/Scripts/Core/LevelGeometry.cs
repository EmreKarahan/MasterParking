using System;
using System.Collections.Generic;

namespace MasterParking.Core
{
    /// <summary>Computed extents of a level, used by the overview camera.</summary>
    public struct LevelBounds
    {
        public float MinX;
        public float MinZ;
        public float MaxX;
        public float MaxZ;

        public float CenterX => (MinX + MaxX) * 0.5f;
        public float CenterZ => (MinZ + MaxZ) * 0.5f;
        public float Width => Math.Max(0.1f, MaxX - MinX);
        public float Depth => Math.Max(0.1f, MaxZ - MinZ);
    }

    /// <summary>Pure level helpers: bounds, validation and spawn sanity checks.</summary>
    public static class LevelGeometry
    {
        public static LevelBounds CalculateBounds(LevelDefinition level)
        {
            if (level == null)
            {
                return new LevelBounds { MinX = -60f, MaxX = 60f, MinZ = -32.5f, MaxZ = 32.5f };
            }

            if (level.Walls == null || level.Walls.Count == 0)
            {
                var center = level.Target != null ? level.Target.Center : new Vec2(level.Start.X, level.Start.Z);
                return new LevelBounds
                {
                    MinX = center.X - 30f,
                    MaxX = center.X + 30f,
                    MinZ = center.Y - 20f,
                    MaxZ = center.Y + 20f
                };
            }

            float minX = float.MaxValue;
            float minZ = float.MaxValue;
            float maxX = float.MinValue;
            float maxZ = float.MinValue;

            for (int i = 0; i < level.Walls.Count; i++)
            {
                WallDefinition wall = level.Walls[i];
                minX = Math.Min(minX, wall.X);
                minZ = Math.Min(minZ, wall.Z);
                maxX = Math.Max(maxX, wall.X + wall.Width);
                maxZ = Math.Max(maxZ, wall.Z + wall.Depth);
            }

            return new LevelBounds { MinX = minX, MinZ = minZ, MaxX = maxX, MaxZ = maxZ };
        }

        /// <summary>Returns human-readable data errors. Used by tests and the editor validator.</summary>
        public static List<string> Validate(LevelDefinition level)
        {
            var errors = new List<string>();
            if (level == null)
            {
                errors.Add("Level is null.");
                return errors;
            }

            if (level.Id <= 0)
            {
                errors.Add("Id must be greater than zero.");
            }

            if (string.IsNullOrWhiteSpace(level.Name))
            {
                errors.Add("Name is empty.");
            }

            if (level.ParTime <= 0f)
            {
                errors.Add("ParTime must be positive.");
            }

            if (level.Target == null)
            {
                errors.Add("Target is missing.");
            }
            else
            {
                if (level.Target.Width <= 0f || level.Target.Depth <= 0f)
                {
                    errors.Add("Target size must be positive.");
                }

                if (level.Target.ToleranceDistance <= 0f || level.Target.ToleranceAngle <= 0f)
                {
                    errors.Add("Target tolerances must be positive.");
                }
            }

            if (level.Walls == null)
            {
                errors.Add("Walls list is null.");
            }
            else
            {
                for (int i = 0; i < level.Walls.Count; i++)
                {
                    WallDefinition wall = level.Walls[i];
                    if (wall.Width <= 0f || wall.Depth <= 0f)
                    {
                        errors.Add($"Wall {i} has a non-positive size.");
                    }
                }
            }

            if (level.Cones != null)
            {
                for (int i = 0; i < level.Cones.Count; i++)
                {
                    if (level.Cones[i].Radius <= 0f)
                    {
                        errors.Add($"Cone {i} has a non-positive radius.");
                    }
                }
            }

            return errors;
        }

        /// <summary>True when the point is inside any wall rectangle. Used for spawn validation.</summary>
        public static bool IsInsideAnyWall(LevelDefinition level, Vec2 point, float padding = 0f)
        {
            if (level?.Walls == null)
            {
                return false;
            }

            for (int i = 0; i < level.Walls.Count; i++)
            {
                WallDefinition wall = level.Walls[i];
                if (point.X >= wall.X - padding && point.X <= wall.X + wall.Width + padding &&
                    point.Y >= wall.Z - padding && point.Y <= wall.Z + wall.Depth + padding)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
