using MasterParking.Core;
using UnityEngine;

namespace MasterParking.Gameplay
{
    /// <summary>
    /// Street furniture from the level's decor block. All props are visual only:
    /// gameplay collisions come from walls, cars and cones, as in the source game.
    /// </summary>
    public static class LevelDecorBuilder
    {
        public static void Build(DecorDefinition decor, Transform parent)
        {
            if (decor == null || parent == null)
            {
                return;
            }

            var signs = decor.Signs;
            for (int i = 0; i < signs.Count; i++)
            {
                BuildSign(signs[i], parent, i);
            }

            var lights = decor.Lights;
            for (int i = 0; i < lights.Count; i++)
            {
                BuildTrafficLight(lights[i], parent, i);
            }

            var lamps = decor.Lamps;
            for (int i = 0; i < lamps.Count; i++)
            {
                BuildLamp(lamps[i], parent, i);
            }

            var trees = decor.Trees;
            for (int i = 0; i < trees.Count; i++)
            {
                BuildTree(trees[i], parent, i);
            }

            var bumps = decor.Bumps;
            for (int i = 0; i < bumps.Count; i++)
            {
                BuildBump(bumps[i], parent, i);
            }

            var chevrons = decor.Chevrons;
            for (int i = 0; i < chevrons.Count; i++)
            {
                BuildChevron(chevrons[i], parent, i);
            }

            var gates = decor.Gates;
            for (int i = 0; i < gates.Count; i++)
            {
                BuildGate(gates[i], parent, i);
            }
        }

        private static void BuildSign(SignProp prop, Transform parent, int index)
        {
            float yaw = -prop.Angle * Mathf.Rad2Deg;
            var root = new GameObject("Sign_" + index);
            root.transform.SetParent(parent, false);
            root.transform.localPosition = new Vector3(prop.X, 0f, prop.Z);
            root.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);

            Primitive(PrimitiveType.Cylinder, root.transform, "Post",
                new Vector3(0f, 1.5f, 0f), Quaternion.identity, new Vector3(0.12f, 1.5f, 0.12f),
                WorldMaterials.Metal);

            Primitive(PrimitiveType.Quad, root.transform, "Face",
                new Vector3(0f, 3.2f, 0f), Quaternion.identity, new Vector3(2.1f, 2.1f, 1f),
                WorldMaterials.SignMaterial(prop.Kind), false);
        }

        private static void BuildTrafficLight(TrafficLightProp prop, Transform parent, int index)
        {
            float yaw = -prop.Angle * Mathf.Rad2Deg;
            var root = new GameObject("TrafficLight_" + index);
            root.transform.SetParent(parent, false);
            root.transform.localPosition = new Vector3(prop.X, 0f, prop.Z);
            root.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);

            Primitive(PrimitiveType.Cylinder, root.transform, "Post",
                new Vector3(0f, 1.9f, 0f), Quaternion.identity, new Vector3(0.14f, 1.9f, 0.14f),
                WorldMaterials.Metal);
            Primitive(PrimitiveType.Cube, root.transform, "Head",
                new Vector3(0f, 3.9f, 0f), Quaternion.identity, new Vector3(0.55f, 1.45f, 0.4f),
                WorldMaterials.Dark);

            string state = (prop.State ?? string.Empty).ToLowerInvariant();
            BuildBulb(root.transform, 4.35f, state == "red" ? WorldMaterials.LightRed : WorldMaterials.LightOff);
            BuildBulb(root.transform, 3.9f, state == "yellow" ? WorldMaterials.LightYellow : WorldMaterials.LightOff);
            BuildBulb(root.transform, 3.45f, state == "green" ? WorldMaterials.LightGreen : WorldMaterials.LightOff);
        }

        private static void BuildBulb(Transform parent, float y, Material material)
        {
            Primitive(PrimitiveType.Sphere, parent, "Bulb",
                new Vector3(0f, y, -0.22f), Quaternion.identity, new Vector3(0.3f, 0.3f, 0.16f),
                material);
        }

        private static void BuildLamp(LampProp prop, Transform parent, int index)
        {
            float yaw = -prop.Angle * Mathf.Rad2Deg;
            var root = new GameObject("Lamp_" + index);
            root.transform.SetParent(parent, false);
            root.transform.localPosition = new Vector3(prop.X, 0f, prop.Z);
            root.transform.localRotation = Quaternion.Euler(0f, yaw, 0f);

            Primitive(PrimitiveType.Cylinder, root.transform, "Pole",
                new Vector3(0f, 3f, 0f), Quaternion.identity, new Vector3(0.16f, 3f, 0.16f),
                WorldMaterials.Metal);
            Primitive(PrimitiveType.Sphere, root.transform, "Bulb",
                new Vector3(0.7f, 6.1f, 0f), Quaternion.identity, new Vector3(0.55f, 0.55f, 0.55f),
                WorldMaterials.Lamp);
        }

        private static void BuildTree(TreeProp prop, Transform parent, int index)
        {
            float scale = Mathf.Max(0.4f, prop.Scale <= 0f ? 1f : prop.Scale);
            var root = new GameObject("Tree_" + index);
            root.transform.SetParent(parent, false);
            root.transform.localPosition = new Vector3(prop.X, 0f, prop.Z);

            Primitive(PrimitiveType.Cylinder, root.transform, "Trunk",
                new Vector3(0f, 1.4f * scale, 0f), Quaternion.identity,
                new Vector3(0.38f * scale, 1.4f * scale, 0.38f * scale), WorldMaterials.Trunk);
            Primitive(PrimitiveType.Sphere, root.transform, "Crown",
                new Vector3(0f, 3.5f * scale, 0f), Quaternion.identity,
                new Vector3(2.4f * scale, 2.2f * scale, 2.4f * scale), WorldMaterials.Foliage);
        }

        private static void BuildBump(BumpProp prop, Transform parent, int index)
        {
            Primitive(PrimitiveType.Cube, parent, "Bump_" + index,
                new Vector3(prop.X, 0.06f, prop.Z),
                Quaternion.Euler(0f, -prop.Angle * Mathf.Rad2Deg, 0f),
                new Vector3(Mathf.Max(1f, prop.Length), 0.12f, 0.85f),
                WorldMaterials.WallStripe);
        }

        private static void BuildChevron(ChevronProp prop, Transform parent, int index)
        {
            var root = new GameObject("Chevron_" + index);
            root.transform.SetParent(parent, false);
            root.transform.localPosition = new Vector3(prop.X, 0f, prop.Z);
            root.transform.localRotation = Quaternion.Euler(0f, -prop.Angle * Mathf.Rad2Deg, 0f);

            for (int i = -1; i <= 1; i++)
            {
                Primitive(PrimitiveType.Cube, root.transform, "Stripe",
                    new Vector3(i * 1.1f, 0.55f, 0f),
                    Quaternion.Euler(0f, 0f, 38f),
                    new Vector3(0.42f, 1.1f, 0.14f),
                    WorldMaterials.WallStripe);
            }
        }

        private static void BuildGate(GateProp prop, Transform parent, int index)
        {
            var root = new GameObject("Gate_" + index);
            root.transform.SetParent(parent, false);
            root.transform.localPosition = new Vector3(prop.X, 0f, prop.Z);
            root.transform.localRotation = Quaternion.Euler(0f, -prop.Angle * Mathf.Rad2Deg, 0f);

            Primitive(PrimitiveType.Cube, root.transform, "PostL",
                new Vector3(0f, 2f, -3.6f), Quaternion.identity, new Vector3(0.5f, 4f, 0.5f),
                WorldMaterials.Dark);
            Primitive(PrimitiveType.Cube, root.transform, "PostR",
                new Vector3(0f, 2f, 3.6f), Quaternion.identity, new Vector3(0.5f, 4f, 0.5f),
                WorldMaterials.Dark);
            Primitive(PrimitiveType.Cube, root.transform, "Beam",
                new Vector3(0f, 4f, 0f), Quaternion.identity, new Vector3(0.65f, 0.55f, 7.7f),
                WorldMaterials.WallStripe);
        }

        private static GameObject Primitive(
            PrimitiveType type,
            Transform parent,
            string name,
            Vector3 localPosition,
            Quaternion localRotation,
            Vector3 localScale,
            Material material,
            bool keepCollider = false)
        {
            GameObject primitive = GameObject.CreatePrimitive(type);
            primitive.name = name;

            if (!keepCollider)
            {
                Collider collider = primitive.GetComponent<Collider>();
                if (collider != null)
                {
                    Object.Destroy(collider);
                }
            }

            Transform transform = primitive.transform;
            transform.SetParent(parent, false);
            transform.localPosition = localPosition;
            transform.localRotation = localRotation;
            transform.localScale = localScale;

            var renderer = primitive.GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.sharedMaterial = material;
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.receiveShadows = true;
            }

            return primitive;
        }
    }
}
