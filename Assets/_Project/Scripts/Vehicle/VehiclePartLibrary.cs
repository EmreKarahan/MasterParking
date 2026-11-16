using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace MasterParking.Vehicle
{
    [Serializable]
    public sealed class VehiclePivotData
    {
        public float X;
        public float Y;
        public float Z;
    }

    [Serializable]
    public sealed class VehiclePartInfo
    {
        public string Name;
        public string File;
        public bool Glass;
        public VehiclePivotData Pivot;
    }

    [Serializable]
    public sealed class VehiclePartCollection
    {
        public List<VehiclePartInfo> Parts = new List<VehiclePartInfo>();
    }

    /// <summary>One loaded car part, ready to be instanced into a car.</summary>
    public sealed class VehiclePart
    {
        public string Name;
        public Mesh Mesh;
        public bool Glass;
        public Vector3 Pivot;
        public bool IsWheel;
        public bool IsFrontWheel;
        public float WheelRadius;
    }

    /// <summary>
    /// Loads the twelve extracted OBJ parts once. The browser game embedded a
    /// single model; splitting it here lets the wheels steer and roll.
    /// </summary>
    public static class VehiclePartLibrary
    {
        public const string PartsResourcePath = "Vehicles/vehicle-parts";

        private static List<VehiclePart> _parts;
        private static bool _attempted;

        public static IReadOnlyList<VehiclePart> GetParts()
        {
            if (!_attempted)
            {
                _attempted = true;
                _parts = LoadParts();
            }

            return _parts;
        }

        private static List<VehiclePart> LoadParts()
        {
            var result = new List<VehiclePart>();

            TextAsset json = Resources.Load<TextAsset>(PartsResourcePath);
            if (json == null)
            {
                Debug.LogWarning("[Vehicle] vehicle-parts.json missing; using placeholder car.");
                return result;
            }

            VehiclePartCollection collection;
            try
            {
                collection = JsonUtility.FromJson<VehiclePartCollection>(json.text);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("[Vehicle] Could not parse part list: " + exception.Message);
                return result;
            }

            if (collection?.Parts == null)
            {
                return result;
            }

            for (int i = 0; i < collection.Parts.Count; i++)
            {
                VehiclePartInfo info = collection.Parts[i];
                if (info == null || string.IsNullOrEmpty(info.File))
                {
                    continue;
                }

                string fileWithoutExtension = Path.GetFileNameWithoutExtension(info.File);
                string resourcePath = "Vehicles/" + fileWithoutExtension;
                Mesh mesh = ObjMeshLoader.LoadFromResource(resourcePath, info.Name ?? fileWithoutExtension);
                if (mesh == null)
                {
                    Debug.LogWarning("[Vehicle] Missing mesh resource: " + resourcePath);
                    continue;
                }

                Vector3 pivot = info.Pivot != null
                    ? new Vector3(info.Pivot.X, info.Pivot.Y, info.Pivot.Z)
                    : Vector3.zero;

                string logicalName = info.Name ?? fileWithoutExtension;
                bool isWheel = logicalName.StartsWith("wheel", StringComparison.OrdinalIgnoreCase);
                bool isFront = logicalName.EndsWith("_fl", StringComparison.OrdinalIgnoreCase) ||
                               logicalName.EndsWith("_fr", StringComparison.OrdinalIgnoreCase);

                result.Add(new VehiclePart
                {
                    Name = logicalName,
                    Mesh = mesh,
                    Glass = info.Glass,
                    Pivot = pivot,
                    IsWheel = isWheel,
                    IsFrontWheel = isFront,
                    WheelRadius = isWheel ? Mathf.Clamp(mesh.bounds.extents.y, 0.3f, 1.4f) : 0.7f
                });
            }

            return result;
        }
    }
}
