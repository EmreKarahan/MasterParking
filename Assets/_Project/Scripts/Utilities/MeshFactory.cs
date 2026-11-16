using System.Collections.Generic;
using MasterParking.Core;
using UnityEngine;

namespace MasterParking.Utilities
{
    /// <summary>
    /// Procedural meshes that would otherwise be many GameObjects: road paint,
    /// all traffic cones, and parking-bay outlines are merged into single meshes.
    /// This is a mobile draw-call decision, not just an artistic one.
    /// </summary>
    public static class MeshFactory
    {
        private const float MarkingY = 0.035f;

        public static Mesh BuildRoadMarkings(LevelDefinition level)
        {
            var vertices = new List<Vector3>(1024);
            var triangles = new List<int>(2048);
            var normals = new List<Vector3>(1024);
            var uvs = new List<Vector2>(1024);
            if (level?.Decor != null)
            {
                DecorDefinition decor = level.Decor;
                for (int i = 0; i < decor.Paths.Count; i++)
                {
                    AddDashedPolyline(vertices, triangles, normals, uvs, decor.Paths[i], 0.26f, 2.0f, 1.6f);
                }

                for (int i = 0; i < decor.Lines.Count; i++)
                {
                    AddSolidPolyline(vertices, triangles, normals, uvs, decor.Lines[i], 0.22f);
                }

                for (int i = 0; i < decor.Zebras.Count; i++)
                {
                    AddZebra(vertices, triangles, normals, uvs, decor.Zebras[i]);
                }

                for (int i = 0; i < decor.Rects.Count; i++)
                {
                    PaintRect rect = decor.Rects[i];
                    AddQuad(
                        vertices, triangles, normals, uvs,
                        new Vector3(rect.X, MarkingY, rect.Z),
                        new Vector3(rect.X + rect.Width, MarkingY, rect.Z),
                        new Vector3(rect.X + rect.Width, MarkingY, rect.Z + rect.Depth),
                        new Vector3(rect.X, MarkingY, rect.Z + rect.Depth));
                }

                for (int i = 0; i < decor.Arrows.Count; i++)
                {
                    AddArrow(vertices, triangles, normals, uvs, decor.Arrows[i]);
                }
            }

            if (level != null)
            {
                for (int i = 0; i < level.ParkedCars.Count; i++)
                {
                    ParkedCarDefinition car = level.ParkedCars[i];
                    AddBayOutline(
                        vertices, triangles, normals, uvs,
                        new Vector3(car.X, MarkingY, car.Z),
                        car.Angle, 8.8f, 4.5f, 0.14f);
                }

                if (level.Target != null)
                {
                    AddBayOutline(
                        vertices, triangles, normals, uvs,
                        new Vector3(level.Target.X, MarkingY, level.Target.Z),
                        level.Target.Angle,
                        level.Target.Width + 0.4f,
                        level.Target.Depth + 0.3f,
                        0.14f);
                }
            }

            if (vertices.Count == 0)
            {
                return null;
            }

            var mesh = new Mesh { name = "RoadMarkings" };
            if (vertices.Count > 65000)
            {
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            }

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateBounds();
            return mesh;
        }

        public static Mesh CreateConeMesh(float radius = 1f, float height = 2.3f, int segments = 14)
        {
            var vertices = new List<Vector3>();
            var triangles = new List<int>();
            var normals = new List<Vector3>();
            var uvs = new List<Vector2>();

            float baseY = 0.5f;
            float topY = height;
            for (int i = 0; i < segments; i++)
            {
                float a0 = i * Mathf.PI * 2f / segments;
                float a1 = (i + 1) * Mathf.PI * 2f / segments;
                Vector3 p0 = new Vector3(Mathf.Cos(a0) * radius, baseY, Mathf.Sin(a0) * radius);
                Vector3 p1 = new Vector3(Mathf.Cos(a1) * radius, baseY, Mathf.Sin(a1) * radius);
                Vector3 top = new Vector3(0f, topY, 0f);
                AddTriangle(vertices, triangles, normals, uvs, p0, p1, top,
                    new Vector3(Mathf.Cos(a0), 0.3f, Mathf.Sin(a0)).normalized);
            }

            // Base slab so the cone reads from the parking camera.
            AddQuad(vertices, triangles, normals, uvs,
                new Vector3(-radius * 1.5f, 0f, -radius * 1.5f),
                new Vector3(radius * 1.5f, 0f, -radius * 1.5f),
                new Vector3(radius * 1.5f, 0f, radius * 1.5f),
                new Vector3(-radius * 1.5f, 0f, radius * 1.5f),
                Vector3.up, 0f);

            var mesh = new Mesh { name = "TrafficCone" };
            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>All level cones in one mesh; colliders are added separately.</summary>
        public static Mesh BuildCones(IList<ConeDefinition> cones, float height = 2.3f)
        {
            if (cones == null || cones.Count == 0)
            {
                return null;
            }

            var vertices = new List<Vector3>(cones.Count * 12 * 6);
            var triangles = new List<int>(cones.Count * 12 * 6);
            var normals = new List<Vector3>(cones.Count * 12 * 6);
            var uvs = new List<Vector2>(cones.Count * 12 * 6);

            const int segments = 12;
            for (int c = 0; c < cones.Count; c++)
            {
                ConeDefinition cone = cones[c];
                float radius = Mathf.Max(0.25f, cone.Radius);
                Vector3 center = new Vector3(cone.X, 0f, cone.Z);
                Vector3 top = center + Vector3.up * height;

                for (int i = 0; i < segments; i++)
                {
                    float a0 = i * Mathf.PI * 2f / segments;
                    float a1 = (i + 1) * Mathf.PI * 2f / segments;
                    Vector3 p0 = center + new Vector3(Mathf.Cos(a0) * radius, 0f, Mathf.Sin(a0) * radius);
                    Vector3 p1 = center + new Vector3(Mathf.Cos(a1) * radius, 0f, Mathf.Sin(a1) * radius);
                    AddRawTriangle(vertices, triangles, normals, uvs, p0, top, p1);
                }

                Vector3 baseCenter = center + Vector3.up * 0.04f;
                AddQuad(
                    vertices, triangles, normals, uvs,
                    baseCenter + new Vector3(-radius * 1.4f, 0f, -radius * 1.4f),
                    baseCenter + new Vector3(radius * 1.4f, 0f, -radius * 1.4f),
                    baseCenter + new Vector3(radius * 1.4f, 0f, radius * 1.4f),
                    baseCenter + new Vector3(-radius * 1.4f, 0f, radius * 1.4f));
            }

            var mesh = new Mesh { name = "TrafficCones" };
            if (vertices.Count > 65000)
            {
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            }

            mesh.SetVertices(vertices);
            mesh.SetTriangles(triangles, 0);
            mesh.SetNormals(normals);
            mesh.SetUVs(0, uvs);
            mesh.RecalculateBounds();
            return mesh;
        }

        public static Mesh Combine(IList<Mesh> meshes, string name)
        {
            var instances = new List<CombineInstance>(meshes?.Count ?? 0);
            if (meshes != null)
            {
                for (int i = 0; i < meshes.Count; i++)
                {
                    if (meshes[i] == null)
                    {
                        continue;
                    }

                    instances.Add(new CombineInstance
                    {
                        mesh = meshes[i],
                        transform = Matrix4x4.identity
                    });
                }
            }

            if (instances.Count == 0)
            {
                return null;
            }

            var combined = new Mesh { name = name };
            combined.indexFormat = UnityEngine.Rendering.IndexFormat.UInt16;
            combined.CombineMeshes(instances.ToArray(), true, false);
            combined.RecalculateBounds();
            return combined;
        }

        private static void AddDashedPolyline(
            List<Vector3> vertices, List<int> triangles, List<Vector3> normals, List<Vector2> uvs,
            PaintPath path, float width, float dash, float gap)
        {
            if (path?.Points == null || path.Points.Count < 2)
            {
                return;
            }

            for (int i = 0; i < path.Points.Count - 1; i++)
            {
                Vector3 start = new Vector3(path.Points[i].X, MarkingY, path.Points[i].Z);
                Vector3 end = new Vector3(path.Points[i + 1].X, MarkingY, path.Points[i + 1].Z);
                float length = Vector3.Distance(start, end);
                if (length < 0.01f)
                {
                    continue;
                }

                Vector3 direction = (end - start) / length;
                float travelled = 0f;
                while (travelled < length)
                {
                    float segment = Mathf.Min(dash, length - travelled);
                    AddLineSegment(
                        vertices, triangles, normals, uvs,
                        start + direction * travelled,
                        start + direction * (travelled + segment),
                        width);
                    travelled += dash + gap;
                }
            }
        }

        private static void AddSolidPolyline(
            List<Vector3> vertices, List<int> triangles, List<Vector3> normals, List<Vector2> uvs,
            PaintPath path, float width)
        {
            if (path?.Points == null || path.Points.Count < 2)
            {
                return;
            }

            for (int i = 0; i < path.Points.Count - 1; i++)
            {
                AddLineSegment(
                    vertices, triangles, normals, uvs,
                    new Vector3(path.Points[i].X, MarkingY, path.Points[i].Z),
                    new Vector3(path.Points[i + 1].X, MarkingY, path.Points[i + 1].Z),
                    width);
            }
        }

        private static void AddZebra(
            List<Vector3> vertices, List<int> triangles, List<Vector3> normals, List<Vector2> uvs,
            PaintZebra zebra)
        {
            const float stripe = 0.5f;
            const float gap = 0.5f;
            if (zebra.Axis == "y")
            {
                for (float x = zebra.X + 0.3f; x + stripe <= zebra.X + zebra.Width - 0.2f; x += stripe + gap)
                {
                    AddQuad(
                        vertices, triangles, normals, uvs,
                        new Vector3(x, MarkingY, zebra.Z),
                        new Vector3(x + stripe, MarkingY, zebra.Z),
                        new Vector3(x + stripe, MarkingY, zebra.Z + zebra.Depth),
                        new Vector3(x, MarkingY, zebra.Z + zebra.Depth));
                }
            }
            else
            {
                for (float z = zebra.Z + 0.3f; z + stripe <= zebra.Z + zebra.Depth - 0.2f; z += stripe + gap)
                {
                    AddQuad(
                        vertices, triangles, normals, uvs,
                        new Vector3(zebra.X, MarkingY, z),
                        new Vector3(zebra.X + zebra.Width, MarkingY, z),
                        new Vector3(zebra.X + zebra.Width, MarkingY, z + stripe),
                        new Vector3(zebra.X, MarkingY, z + stripe));
                }
            }
        }

        private static void AddArrow(
            List<Vector3> vertices, List<int> triangles, List<Vector3> normals, List<Vector2> uvs,
            PaintArrow arrow)
        {
            Vector3 direction = new Vector3(Mathf.Cos(arrow.Angle), 0f, Mathf.Sin(arrow.Angle));
            Vector3 right = new Vector3(direction.z, 0f, -direction.x);
            Vector3 center = new Vector3(arrow.X, MarkingY, arrow.Z);

            Vector3 shaftStart = center - direction * 1.6f;
            Vector3 shaftEnd = center + direction * 0.4f;
            AddLineSegment(vertices, triangles, normals, uvs, shaftStart, shaftEnd, 0.75f);

            Vector3 tip = center + direction * 2.2f;
            Vector3 leftBase = center + direction * 0.4f + right * 1.0f;
            Vector3 rightBase = center + direction * 0.4f - right * 1.0f;
            AddTriangle(vertices, triangles, normals, uvs, tip, leftBase, rightBase, Vector3.up);
        }

        private static void AddBayOutline(
            List<Vector3> vertices, List<int> triangles, List<Vector3> normals, List<Vector2> uvs,
            Vector3 center, float angle, float width, float depth, float thickness)
        {
            Vector3 forward = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            Vector3 right = new Vector3(forward.z, 0f, -forward.x);
            Vector3 halfForward = forward * (depth * 0.5f);
            Vector3 halfRight = right * (width * 0.5f);

            Vector3 a = center - halfForward - halfRight;
            Vector3 b = center - halfForward + halfRight;
            Vector3 c = center + halfForward + halfRight;
            Vector3 d = center + halfForward - halfRight;

            AddLineSegment(vertices, triangles, normals, uvs, a, b, thickness);
            AddLineSegment(vertices, triangles, normals, uvs, b, c, thickness);
            AddLineSegment(vertices, triangles, normals, uvs, c, d, thickness);
            AddLineSegment(vertices, triangles, normals, uvs, d, a, thickness);
        }

        private static void AddLineSegment(
            List<Vector3> vertices, List<int> triangles, List<Vector3> normals, List<Vector2> uvs,
            Vector3 start, Vector3 end, float width)
        {
            Vector3 delta = end - start;
            delta.y = 0f;
            if (delta.sqrMagnitude < 1e-6f)
            {
                return;
            }

            Vector3 direction = delta.normalized;
            Vector3 right = new Vector3(-direction.z, 0f, direction.x) * (width * 0.5f);
            AddQuad(vertices, triangles, normals, uvs,
                start + right, start - right, end - right, end + right);
        }

        private static void AddQuad(
            List<Vector3> vertices, List<int> triangles, List<Vector3> normals, List<Vector2> uvs,
            Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
        {
            int index = vertices.Count;
            vertices.Add(p0);
            vertices.Add(p1);
            vertices.Add(p2);
            vertices.Add(p3);

            EnsureUpward(ref p1, ref p2, ref p3, index, vertices);

            for (int i = 0; i < 4; i++)
            {
                normals.Add(Vector3.up);
            }

            uvs.Add(new Vector2(0f, 0f));
            uvs.Add(new Vector2(1f, 0f));
            uvs.Add(new Vector2(1f, 1f));
            uvs.Add(new Vector2(0f, 1f));

            triangles.Add(index);
            triangles.Add(index + 1);
            triangles.Add(index + 2);
            triangles.Add(index);
            triangles.Add(index + 2);
            triangles.Add(index + 3);
        }

        private static void AddQuad(
            List<Vector3> vertices, List<int> triangles, List<Vector3> normals, List<Vector2> uvs,
            Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3, Vector3 normal, float unused)
        {
            int index = vertices.Count;
            Vector3 a = p0;
            Vector3 b = p1;
            Vector3 c = p2;
            Vector3 d = p3;

            Vector3 cross = Vector3.Cross(b - a, c - a);
            if (cross.y < 0f)
            {
                Vector3 temp = b;
                b = d;
                d = temp;
            }

            vertices.Add(a);
            vertices.Add(b);
            vertices.Add(c);
            vertices.Add(d);
            for (int i = 0; i < 4; i++)
            {
                normals.Add(normal);
            }

            uvs.Add(new Vector2(0f, 0f));
            uvs.Add(new Vector2(1f, 0f));
            uvs.Add(new Vector2(1f, 1f));
            uvs.Add(new Vector2(0f, 1f));

            triangles.Add(index);
            triangles.Add(index + 1);
            triangles.Add(index + 2);
            triangles.Add(index);
            triangles.Add(index + 2);
            triangles.Add(index + 3);
        }

        private static void AddRawTriangle(
            List<Vector3> vertices, List<int> triangles, List<Vector3> normals, List<Vector2> uvs,
            Vector3 a, Vector3 b, Vector3 c)
        {
            int index = vertices.Count;
            Vector3 normal = Vector3.Cross(b - a, c - a).normalized;
            vertices.Add(a);
            vertices.Add(b);
            vertices.Add(c);
            normals.Add(normal);
            normals.Add(normal);
            normals.Add(normal);
            uvs.Add(new Vector2(0f, 0f));
            uvs.Add(new Vector2(1f, 0f));
            uvs.Add(new Vector2(0.5f, 1f));
            triangles.Add(index);
            triangles.Add(index + 1);
            triangles.Add(index + 2);
        }

        private static void AddTriangle(
            List<Vector3> vertices, List<int> triangles, List<Vector3> normals, List<Vector2> uvs,
            Vector3 a, Vector3 b, Vector3 c, Vector3 normal)
        {
            int index = vertices.Count;
            Vector3 cross = Vector3.Cross(b - a, c - a);
            vertices.Add(a);
            if (cross.y < 0f)
            {
                vertices.Add(c);
                vertices.Add(b);
            }
            else
            {
                vertices.Add(b);
                vertices.Add(c);
            }

            for (int i = 0; i < 3; i++)
            {
                normals.Add(normal);
            }

            uvs.Add(new Vector2(0f, 0f));
            uvs.Add(new Vector2(1f, 0f));
            uvs.Add(new Vector2(0.5f, 1f));
            triangles.Add(index);
            triangles.Add(index + 1);
            triangles.Add(index + 2);
        }

        private static void EnsureUpward(
            ref Vector3 p1, ref Vector3 p2, ref Vector3 p3, int index, List<Vector3> vertices)
        {
            if (vertices.Count < index + 4)
            {
                return;
            }

            Vector3 a = vertices[index];
            Vector3 b = vertices[index + 1];
            Vector3 c = vertices[index + 2];
            if (Vector3.Cross(b - a, c - a).y < 0f)
            {
                Vector3 temp = vertices[index + 1];
                vertices[index + 1] = vertices[index + 3];
                vertices[index + 3] = temp;
            }
        }
    }
}
