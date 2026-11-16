using System.Collections.Generic;
using MasterParking.Utilities;
using UnityEngine;

namespace MasterParking.Vehicle
{
    /// <summary>
    /// Builds player, parked and traffic cars from the extracted parts. If the
    /// Resources payload is unavailable (for example in a trimmed test project),
    /// a clearly readable primitive car is used instead of failing.
    /// </summary>
    public static class VehicleFactory
    {
        public const float ColliderLength = 7.5f;
        public const float ColliderWidth = 3.4f;
        public const float ColliderHeight = 3.2f;

        public static GameObject Create(
            Transform parent,
            string name,
            Vector3 position,
            Quaternion rotation,
            Color tint,
            bool withView)
        {
            var root = new GameObject(name);
            root.transform.SetParent(parent, false);
            root.transform.localPosition = position;
            root.transform.localRotation = rotation;

            VehicleView view = withView ? root.AddComponent<VehicleView>() : null;
            BuildParts(root.transform, tint, view);
            return root;
        }

        public static BoxCollider AddObstacleCollider(GameObject root)
        {
            var collider = root.AddComponent<BoxCollider>();
            collider.center = new Vector3(0f, ColliderHeight * 0.5f, 0f);
            collider.size = new Vector3(ColliderLength, ColliderHeight, ColliderWidth);
            return collider;
        }

        private static void BuildParts(Transform root, Color tint, VehicleView view)
        {
            IReadOnlyList<VehiclePart> parts = VehiclePartLibrary.GetParts();
            if (parts == null || parts.Count == 0)
            {
                BuildPlaceholder(root, tint, view);
                return;
            }

            var wheels = new List<Transform>(4);
            var frontWheels = new List<bool>(4);
            float radius = 0.7f;

            Material bodyMaterial = CarMaterialLibrary.Body(tint);
            Material glassMaterial = CarMaterialLibrary.Glass();

            for (int i = 0; i < parts.Count; i++)
            {
                VehiclePart part = parts[i];
                var partObject = new GameObject(part.Name);
                Transform partTransform = partObject.transform;
                partTransform.SetParent(root, false);
                partTransform.localPosition = part.IsWheel ? part.Pivot : Vector3.zero;
                partTransform.localRotation = Quaternion.identity;

                var filter = partObject.AddComponent<MeshFilter>();
                filter.sharedMesh = part.Mesh;

                var renderer = partObject.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = part.Glass ? glassMaterial : bodyMaterial;
                MaterialLibrary.SetShadowMode(renderer, true, true);

                if (part.IsWheel)
                {
                    wheels.Add(partTransform);
                    frontWheels.Add(part.IsFrontWheel);
                    radius = part.WheelRadius;
                }
            }

            if (view != null)
            {
                view.Configure(wheels.ToArray(), frontWheels.ToArray(), radius);
            }

            if (view == null)
            {
                // Parked/traffic cars do not need the per-frame view script, but
                // their wheels should still face the right way for visuals.
                for (int i = 0; i < wheels.Count; i++)
                {
                    wheels[i].localRotation = Quaternion.identity;
                }
            }
        }

        private static void BuildPlaceholder(Transform root, Color tint, VehicleView view)
        {
            Material material = CarMaterialLibrary.Placeholder(tint);
            Material glass = CarMaterialLibrary.Glass();

            CreateBox(root, "Body", new Vector3(0f, 0.85f, 0f), new Vector3(7.2f, 1.1f, 3.2f), material);
            CreateBox(root, "Cabin", new Vector3(-0.3f, 1.95f, 0f), new Vector3(3.6f, 1.1f, 2.9f), glass);

            var wheels = new Transform[4];
            var front = new bool[4];
            float[] z = { -1.7f, 1.7f, -1.7f, 1.7f };
            float[] x = { 2.45f, 2.45f, -2.55f, -2.55f };

            for (int i = 0; i < 4; i++)
            {
                var pivot = new GameObject("Wheel_" + i);
                pivot.transform.SetParent(root, false);
                pivot.transform.localPosition = new Vector3(x[i], 0.7f, z[i]);
                CreateCylinder(pivot.transform, material);
                wheels[i] = pivot.transform;
                front[i] = x[i] > 0f;
            }

            view?.Configure(wheels, front, 0.7f);
        }

        private static void CreateBox(Transform parent, string name, Vector3 center, Vector3 size, Material material)
        {
            GameObject box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            Object.Destroy(box.GetComponent<Collider>());
            box.transform.SetParent(parent, false);
            box.transform.localPosition = center;
            box.transform.localScale = size;
            box.GetComponent<MeshRenderer>().sharedMaterial = material;
        }

        private static void CreateCylinder(Transform parent, Material material)
        {
            GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            Object.Destroy(cylinder.GetComponent<Collider>());
            cylinder.transform.SetParent(parent, false);
            cylinder.transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            cylinder.transform.localScale = new Vector3(1.4f, 0.42f, 1.4f);
            cylinder.GetComponent<MeshRenderer>().sharedMaterial = material;
        }
    }
}
