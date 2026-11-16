using System.Collections.Generic;
using MasterParking.Core;
using MasterParking.Utilities;
using MasterParking.Vehicle;
using UnityEngine;
using UnityEngine.Rendering;

namespace MasterParking.Gameplay
{
    /// <summary>
    /// Turns a LevelDefinition into a playable scene. No level-specific branches:
    /// every level uses the same walls/cars/cones/movers/decor pipeline.
    /// </summary>
    public static class LevelBuilder
    {
        private static readonly Color[] ParkedCarTints =
        {
            new Color(0.78f, 0.29f, 0.26f),
            new Color(0.22f, 0.45f, 0.72f),
            new Color(0.85f, 0.7f, 0.25f),
            new Color(0.35f, 0.65f, 0.48f),
            new Color(0.62f, 0.62f, 0.66f)
        };

        public static LevelWorld Build(LevelDefinition level, Transform parent)
        {
            var world = new LevelWorld
            {
                Level = level,
                Obstacles = new ObstacleRegistry(),
                Bounds = LevelGeometry.CalculateBounds(level)
            };

            if (level == null)
            {
                Debug.LogError("[Level] Cannot build a null level.");
                return world;
            }

            world.Root = new GameObject("Level_" + level.Id + "_" + level.Name);
            world.Root.transform.SetParent(parent, false);

            var staticRoot = new GameObject("Static").transform;
            staticRoot.SetParent(world.Root.transform, false);

            var dynamicRoot = new GameObject("Dynamic").transform;
            dynamicRoot.SetParent(world.Root.transform, false);

            BuildGround(level, staticRoot, world.Bounds);
            BuildRoadMarkings(level, staticRoot);
            BuildWalls(level, staticRoot, world);
            BuildParkedCars(level, staticRoot, world);
            BuildCones(level, staticRoot, world);
            BuildMovers(level, dynamicRoot, world);
            BuildTarget(level, staticRoot, dynamicRoot, world);
            LevelDecorBuilder.Build(level.Decor, staticRoot);
            BuildLighting(world.Root.transform);

            StaticBatchingUtility.Combine(staticRoot.gameObject);
            return world;
        }

        private static void BuildGround(LevelDefinition level, Transform parent, LevelBounds bounds)
        {
            float width = Mathf.Max(130f, bounds.Width + 36f);
            float depth = Mathf.Max(80f, bounds.Depth + 30f);

            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "Ground";
            Object.Destroy(ground.GetComponent<Collider>());
            ground.transform.SetParent(parent, false);
            ground.transform.localPosition = new Vector3(bounds.CenterX, 0f, bounds.CenterZ);
            ground.transform.localScale = new Vector3(width / 10f, 1f, depth / 10f);

            var renderer = ground.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = WorldMaterials.Asphalt;
            MaterialLibrary.SetShadowMode(renderer, false, true);
        }

        private static void BuildRoadMarkings(LevelDefinition level, Transform parent)
        {
            Mesh mesh = MeshFactory.BuildRoadMarkings(level);
            if (mesh == null)
            {
                return;
            }

            var marking = new GameObject("RoadMarkings");
            marking.transform.SetParent(parent, false);
            marking.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = marking.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = WorldMaterials.Paint;
            MaterialLibrary.SetShadowMode(renderer, false, false);
        }

        private static void BuildWalls(LevelDefinition level, Transform parent, LevelWorld world)
        {
            if (level.Walls == null)
            {
                return;
            }

            for (int i = 0; i < level.Walls.Count; i++)
            {
                WallDefinition wall = level.Walls[i];
                var wallObject = GameObject.CreatePrimitive(PrimitiveType.Cube);
                wallObject.name = "Wall_" + i;
                wallObject.transform.SetParent(parent, false);
                wallObject.transform.localPosition =
                    new Vector3(wall.X + wall.Width * 0.5f, 0.75f, wall.Z + wall.Depth * 0.5f);
                wallObject.transform.localScale = new Vector3(wall.Width, 1.5f, wall.Depth);

                var renderer = wallObject.GetComponent<MeshRenderer>();
                renderer.sharedMaterial = WorldMaterials.Wall;
                MaterialLibrary.SetShadowMode(renderer, true, true);

                Collider collider = wallObject.GetComponent<Collider>();
                world.Obstacles.Register(collider, ObstacleType.Wall);

                var cap = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cap.name = "WallCap";
                Object.Destroy(cap.GetComponent<Collider>());
                cap.transform.SetParent(wallObject.transform, false);
                cap.transform.localPosition = new Vector3(0f, 0.51f, 0f);
                cap.transform.localScale = new Vector3(1f, 0.035f, 1f);
                var capRenderer = cap.GetComponent<MeshRenderer>();
                capRenderer.sharedMaterial = WorldMaterials.WallStripe;
                MaterialLibrary.SetShadowMode(capRenderer, false, true);
            }
        }

        private static void BuildParkedCars(LevelDefinition level, Transform parent, LevelWorld world)
        {
            if (level.ParkedCars == null)
            {
                return;
            }

            for (int i = 0; i < level.ParkedCars.Count; i++)
            {
                ParkedCarDefinition car = level.ParkedCars[i];
                Color tint = ColorUtil.Hex(car.Color, ParkedCarTints[i % ParkedCarTints.Length]);
                GameObject carObject = VehicleFactory.Create(
                    parent,
                    "ParkedCar_" + i,
                    new Vector3(car.X, 0f, car.Z),
                    MathConversions.HeadingToRotation(car.Angle),
                    tint,
                    false);

                BoxCollider collider = VehicleFactory.AddObstacleCollider(carObject);
                world.Obstacles.Register(collider, ObstacleType.ParkedCar);
            }
        }

        private static void BuildCones(LevelDefinition level, Transform parent, LevelWorld world)
        {
            if (level.Cones == null || level.Cones.Count == 0)
            {
                return;
            }

            Mesh cones = MeshFactory.BuildCones(level.Cones);
            if (cones != null)
            {
                var coneObject = new GameObject("Cones");
                coneObject.transform.SetParent(parent, false);
                coneObject.AddComponent<MeshFilter>().sharedMesh = cones;
                var renderer = coneObject.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = WorldMaterials.Cone;
                MaterialLibrary.SetShadowMode(renderer, true, true);
            }

            for (int i = 0; i < level.Cones.Count; i++)
            {
                ConeDefinition cone = level.Cones[i];
                var body = new GameObject("ConeCollider_" + i);
                body.transform.SetParent(parent, false);
                body.transform.localPosition = new Vector3(cone.X, 1.15f, cone.Z);
                var collider = body.AddComponent<BoxCollider>();
                float diameter = Mathf.Max(0.5f, cone.Radius * 2f);
                collider.size = new Vector3(diameter, 2.3f, diameter);
                world.Obstacles.Register(collider, ObstacleType.Cone);
            }
        }

        private static void BuildMovers(LevelDefinition level, Transform parent, LevelWorld world)
        {
            if (level.Movers == null)
            {
                return;
            }

            for (int i = 0; i < level.Movers.Count; i++)
            {
                MoverDefinition mover = level.Movers[i];
                float heading = Mathf.Atan2(mover.DirectionZ, mover.DirectionX);
                Color tint = ColorUtil.Hex(mover.Color, ParkedCarTints[(i + 2) % ParkedCarTints.Length]);
                GameObject carObject = VehicleFactory.Create(
                    parent,
                    "MovingCar_" + i,
                    new Vector3(mover.X, 0f, mover.Z),
                    MathConversions.HeadingToRotation(heading),
                    tint,
                    false);

                BoxCollider collider = VehicleFactory.AddObstacleCollider(carObject);
                world.Obstacles.Register(collider, ObstacleType.MovingCar);
                world.Traffic.Add(new TrafficController(mover, carObject));
            }
        }

        private static void BuildTarget(LevelDefinition level, Transform staticRoot, Transform dynamicRoot, LevelWorld world)
        {
            ParkingTarget target = level.Target;
            if (target == null)
            {
                return;
            }

            GameObject decal = GameObject.CreatePrimitive(PrimitiveType.Plane);
            decal.name = "ParkingDecal";
            Object.Destroy(decal.GetComponent<Collider>());
            decal.transform.SetParent(staticRoot, false);
            decal.transform.localPosition = new Vector3(target.X, 0.055f, target.Z);
            decal.transform.localRotation = Quaternion.Euler(0f, -target.Angle * Mathf.Rad2Deg, 0f);
            decal.transform.localScale = new Vector3(target.Width / 10f, 1f, target.Depth / 10f);

            var renderer = decal.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = WorldMaterials.ParkingDecal;
            MaterialLibrary.SetShadowMode(renderer, false, false);

            var marker = GameObject.CreatePrimitive(PrimitiveType.Cube);
            marker.name = "ParkingMarker";
            Object.Destroy(marker.GetComponent<Collider>());
            marker.transform.SetParent(dynamicRoot, false);
            marker.transform.localPosition = new Vector3(target.X, 2.4f, target.Z);
            marker.transform.localRotation = Quaternion.Euler(0f, 45f, 45f);
            marker.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
            var markerRenderer = marker.GetComponent<MeshRenderer>();
            markerRenderer.sharedMaterial = WorldMaterials.Marker;
            MaterialLibrary.SetShadowMode(markerRenderer, false, false);
            marker.AddComponent<ParkingMarkerAnimator>();

            world.TargetMarker = marker.transform;
        }

        private static void BuildLighting(Transform parent)
        {
            var lightObject = new GameObject("Sun");
            lightObject.transform.SetParent(parent, false);
            lightObject.transform.localRotation = Quaternion.Euler(52f, -32f, 0f);

            var light = lightObject.AddComponent<Light>();
            light.type = LightType.Directional;
            light.color = new Color(1f, 0.96f, 0.88f);
            light.intensity = 1.15f;
            light.shadows = LightShadows.Soft;
            light.shadowStrength = 0.75f;
            light.shadowBias = 0.04f;
            light.shadowNormalBias = 0.3f;

            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.42f, 0.46f, 0.54f);
            RenderSettings.fog = false;
            RenderSettings.skybox = null;
            QualitySettings.shadowDistance = 90f;
        }
    }
}
