using System;
using System.Collections.Generic;

namespace MasterParking.Core
{
    /// <summary>
    /// Serializable root for the level JSON/ScriptableObject payload.
    /// Public fields keep Unity's JsonUtility and the inspector happy; the names
    /// intentionally match the PascalCase JSON produced by the migration tool.
    /// </summary>
    [Serializable]
    public sealed class LevelCollection
    {
        public int Version = 1;
        public string Source = string.Empty;
        public float WorldScale = 0.1f;
        public List<LevelDefinition> Levels = new List<LevelDefinition>();
    }

    [Serializable]
    public sealed class LevelDefinition
    {
        public int Id;
        public string Name = string.Empty;
        public float ParTime = 20f;
        public int Difficulty = 1;
        public StartPose Start = new StartPose();
        public ParkingTarget Target = new ParkingTarget();
        public List<WallDefinition> Walls = new List<WallDefinition>();
        public List<ParkedCarDefinition> ParkedCars = new List<ParkedCarDefinition>();
        public List<ConeDefinition> Cones = new List<ConeDefinition>();
        public List<MoverDefinition> Movers = new List<MoverDefinition>();
        public DecorDefinition Decor = new DecorDefinition();
    }

    [Serializable]
    public struct StartPose
    {
        public float X;
        public float Z;
        public float Angle;

        public Vec2 Position => new Vec2(X, Z);
    }

    /// <summary>Axis-aligned wall/curb rectangle. X/Z identify the minimum corner.</summary>
    [Serializable]
    public struct WallDefinition
    {
        public float X;
        public float Z;
        public float Width;
        public float Depth;

        public Vec2 Center => new Vec2(X + Width * 0.5f, Z + Depth * 0.5f);
    }

    [Serializable]
    public struct ParkedCarDefinition
    {
        public float X;
        public float Z;
        public float Angle;
        public string Color;

        public Vec2 Position => new Vec2(X, Z);
    }

    [Serializable]
    public struct ConeDefinition
    {
        public float X;
        public float Z;
        public float Radius;

        public Vec2 Position => new Vec2(X, Z);
    }

    [Serializable]
    public struct MoverDefinition
    {
        public float X;
        public float Z;
        public float DirectionX;
        public float DirectionZ;
        public float MinX;
        public float MaxX;
        public float MinZ;
        public float MaxZ;
        public string Color;

        public Vec2 Position => new Vec2(X, Z);
    }

    [Serializable]
    public sealed class DecorDefinition
    {
        public List<PaintPath> Paths = new List<PaintPath>();
        public List<PaintPath> Lines = new List<PaintPath>();
        public List<PaintZebra> Zebras = new List<PaintZebra>();
        public List<PaintRect> Rects = new List<PaintRect>();
        public List<PaintArrow> Arrows = new List<PaintArrow>();
        public List<SignProp> Signs = new List<SignProp>();
        public List<TrafficLightProp> Lights = new List<TrafficLightProp>();
        public List<LampProp> Lamps = new List<LampProp>();
        public List<TreeProp> Trees = new List<TreeProp>();
        public List<BumpProp> Bumps = new List<BumpProp>();
        public List<ChevronProp> Chevrons = new List<ChevronProp>();
        public List<GateProp> Gates = new List<GateProp>();
    }

    [Serializable]
    public sealed class PaintPath
    {
        public List<PathPoint> Points = new List<PathPoint>();
    }

    [Serializable]
    public struct PathPoint
    {
        public float X;
        public float Z;

        public Vec2 Position => new Vec2(X, Z);
    }

    [Serializable]
    public struct PaintZebra
    {
        public float X;
        public float Z;
        public float Width;
        public float Depth;
        public string Axis;
    }

    [Serializable]
    public struct PaintRect
    {
        public float X;
        public float Z;
        public float Width;
        public float Depth;
    }

    [Serializable]
    public struct PaintArrow
    {
        public float X;
        public float Z;
        public float Angle;
    }

    [Serializable]
    public struct SignProp
    {
        public string Kind;
        public float X;
        public float Z;
        public float Angle;
    }

    [Serializable]
    public struct TrafficLightProp
    {
        public float X;
        public float Z;
        public float Angle;
        public string State;
    }

    [Serializable]
    public struct LampProp
    {
        public float X;
        public float Z;
        public float Angle;
    }

    [Serializable]
    public struct TreeProp
    {
        public float X;
        public float Z;
        public float Scale;
    }

    [Serializable]
    public struct BumpProp
    {
        public float X;
        public float Z;
        public float Length;
        public float Angle;
    }

    [Serializable]
    public struct ChevronProp
    {
        public float X;
        public float Z;
        public float Angle;
    }

    [Serializable]
    public struct GateProp
    {
        public float X;
        public float Z;
        public float Angle;
    }
}
