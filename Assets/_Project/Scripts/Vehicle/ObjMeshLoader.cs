using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace MasterParking.Vehicle
{
    /// <summary>
    /// Minimal OBJ reader for the extracted car. The source meshes are already
    /// triangulated and carry positions, UVs and normals, so a full importer
    /// would be dead weight. Parsing happens once and meshes are cached.
    /// </summary>
    public static class ObjMeshLoader
    {
        public static Mesh LoadFromResource(string resourcePath, string meshName)
        {
            var asset = Resources.Load<TextAsset>(resourcePath);
            if (asset == null)
            {
                return null;
            }

            return Parse(asset.text, meshName);
        }

        public static Mesh Parse(string text, string meshName)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            var positions = new List<Vector3>(2048);
            var uvs = new List<Vector2>(2048);
            var normals = new List<Vector3>(2048);

            var vertices = new List<Vector3>(2048);
            var meshUvs = new List<Vector2>(2048);
            var meshNormals = new List<Vector3>(2048);
            var triangles = new List<int>(2048);

            string[] lines = text.Split('\n');
            for (int lineIndex = 0; lineIndex < lines.Length; lineIndex++)
            {
                string line = lines[lineIndex].Trim();
                if (line.Length < 2)
                {
                    continue;
                }

                if (line[0] == 'v' && line[1] == ' ')
                {
                    positions.Add(ParseVector3(line));
                }
                else if (line[0] == 'v' && line[1] == 't')
                {
                    uvs.Add(ParseVector2(line));
                }
                else if (line[0] == 'v' && line[1] == 'n')
                {
                    normals.Add(ParseVector3(line));
                }
                else if (line[0] == 'f' && line[1] == ' ')
                {
                    string[] corners = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    if (corners.Length < 4)
                    {
                        continue;
                    }

                    for (int i = 2; i < corners.Length - 1; i++)
                    {
                        AddCorner(corners[1], positions, uvs, normals, vertices, meshUvs, meshNormals, triangles);
                        AddCorner(corners[i], positions, uvs, normals, vertices, meshUvs, meshNormals, triangles);
                        AddCorner(corners[i + 1], positions, uvs, normals, vertices, meshUvs, meshNormals, triangles);
                    }
                }
            }

            if (vertices.Count == 0)
            {
                return null;
            }

            var mesh = new Mesh { name = meshName };
            if (vertices.Count > 65000)
            {
                mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;
            }

            mesh.SetVertices(vertices);
            mesh.SetUVs(0, meshUvs);
            mesh.SetTriangles(triangles, 0);

            if (meshNormals.Count == vertices.Count)
            {
                mesh.SetNormals(meshNormals);
            }
            else
            {
                mesh.RecalculateNormals();
            }

            mesh.RecalculateBounds();
            return mesh;
        }

        private static void AddCorner(
            string token,
            List<Vector3> positions,
            List<Vector2> uvs,
            List<Vector3> normals,
            List<Vector3> vertices,
            List<Vector2> meshUvs,
            List<Vector3> meshNormals,
            List<int> triangles)
        {
            string[] parts = token.Split('/');
            int positionIndex = ResolveIndex(parts.Length > 0 ? parts[0] : null, positions.Count);
            if (positionIndex < 0)
            {
                return;
            }

            int vertexIndex = vertices.Count;
            vertices.Add(positions[positionIndex]);

            int uvIndex = parts.Length > 1 ? ResolveIndex(parts[1], uvs.Count) : -1;
            meshUvs.Add(uvIndex >= 0 ? uvs[uvIndex] : Vector2.zero);

            int normalIndex = parts.Length > 2 ? ResolveIndex(parts[2], normals.Count) : -1;
            meshNormals.Add(normalIndex >= 0 ? normals[normalIndex] : Vector3.up);

            triangles.Add(vertexIndex);
        }

        private static int ResolveIndex(string token, int count)
        {
            if (string.IsNullOrEmpty(token) ||
                !int.TryParse(token, NumberStyles.Integer, CultureInfo.InvariantCulture, out int index))
            {
                return -1;
            }

            if (index < 0)
            {
                index = count + index;
            }
            else
            {
                index -= 1;
            }

            return index >= 0 && index < count ? index : -1;
        }

        private static Vector3 ParseVector3(string line)
        {
            string[] parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return new Vector3(
                ParseFloat(parts, 1),
                ParseFloat(parts, 2),
                ParseFloat(parts, 3));
        }

        private static Vector2 ParseVector2(string line)
        {
            string[] parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            return new Vector2(ParseFloat(parts, 1), ParseFloat(parts, 2));
        }

        private static float ParseFloat(string[] parts, int index)
        {
            if (index >= parts.Length)
            {
                return 0f;
            }

            return float.TryParse(parts[index], NumberStyles.Float, CultureInfo.InvariantCulture, out float value)
                ? value
                : 0f;
        }
    }
}
