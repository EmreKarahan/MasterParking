using System;
using System.Collections.Generic;
using UnityEngine;

namespace MasterParking.Utilities
{
    /// <summary>
    /// Small runtime-generated UI/decoration textures. Keeping these procedural
    /// avoids shipping placeholder PNGs and keeps the project free of third-party art.
    /// All results are cached by key for the lifetime of the process.
    /// </summary>
    public static class TextureFactory
    {
        private static readonly Dictionary<string, Sprite> SpriteCache = new Dictionary<string, Sprite>();
        private static readonly Dictionary<string, Texture2D> TextureCache = new Dictionary<string, Texture2D>();

        public static Sprite Solid()
        {
            if (SpriteCache.TryGetValue("solid", out Sprite cached))
            {
                return cached;
            }

            var texture = new Texture2D(4, 4, TextureFormat.RGBA32, false)
            {
                name = "UI_Solid",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            var pixels = new Color32[16];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = new Color32(255, 255, 255, 255);
            }

            texture.SetPixels32(pixels);
            texture.Apply(false, false);

            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 4, 4), new Vector2(0.5f, 0.5f), 100f);
            SpriteCache["solid"] = sprite;
            return sprite;
        }

        public static Sprite RoundedRect(int size = 96, int radius = 24)
        {
            string key = $"round_{size}_{radius}";
            if (SpriteCache.TryGetValue(key, out Sprite cached))
            {
                return cached;
            }

            var pixels = new Color32[size * size];
            int r = Mathf.Clamp(radius, 1, size / 2 - 1);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool inside = IsInsideRoundedRect(x, y, size, size, r);
                    pixels[y * size + x] = inside
                        ? new Color32(255, 255, 255, 255)
                        : new Color32(255, 255, 255, 0);
                }
            }

            Texture2D texture = FromPixels(pixels, size, size, "UI_RoundedRect");
            int border = r + 2;
            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0, 0, size, size),
                new Vector2(0.5f, 0.5f),
                100f,
                0,
                SpriteMeshType.FullRect,
                new Vector4(border, border, border, border));
            SpriteCache[key] = sprite;
            return sprite;
        }

        public static Sprite Circle(int size = 128)
        {
            string key = $"circle_{size}";
            if (SpriteCache.TryGetValue(key, out Sprite cached))
            {
                return cached;
            }

            var pixels = new Color32[size * size];
            float radius = size * 0.5f - 1f;
            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                    pixels[y * size + x] = distance <= radius
                        ? new Color32(255, 255, 255, 255)
                        : new Color32(255, 255, 255, 0);
                }
            }

            Sprite sprite = Sprite.Create(FromPixels(pixels, size, size, "UI_Circle"), new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            SpriteCache[key] = sprite;
            return sprite;
        }

        public static Sprite Ring(int size = 256, float innerRatio = 0.74f)
        {
            string key = $"ring_{size}_{innerRatio:0.00}";
            if (SpriteCache.TryGetValue(key, out Sprite cached))
            {
                return cached;
            }

            var pixels = new Color32[size * size];
            float outer = size * 0.5f - 1f;
            float inner = outer * innerRatio;
            Vector2 center = new Vector2(size * 0.5f, size * 0.5f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                    pixels[y * size + x] = distance <= outer && distance >= inner
                        ? new Color32(255, 255, 255, 255)
                        : new Color32(255, 255, 255, 0);
                }
            }

            Sprite sprite = Sprite.Create(FromPixels(pixels, size, size, "UI_Ring"), new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            SpriteCache[key] = sprite;
            return sprite;
        }

        public static Sprite VerticalGradient(Color top, Color bottom)
        {
            string key = $"grad_{ColorUtility.ToHtmlStringRGB(top)}_{ColorUtility.ToHtmlStringRGB(bottom)}";
            if (SpriteCache.TryGetValue(key, out Sprite cached))
            {
                return cached;
            }

            const int width = 4;
            const int height = 256;
            var pixels = new Color32[width * height];
            for (int y = 0; y < height; y++)
            {
                Color color = Color.Lerp(bottom, top, y / (float)(height - 1));
                var packed = (Color32)color;
                int row = y * width;
                for (int x = 0; x < width; x++)
                {
                    pixels[row + x] = packed;
                }
            }

            Texture2D texture = FromPixels(pixels, width, height, "UI_Gradient");
            texture.wrapMode = TextureWrapMode.Clamp;
            Sprite sprite = Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f), 100f);
            SpriteCache[key] = sprite;
            return sprite;
        }

        public static Sprite Star(int size = 96)
        {
            string key = $"star_{size}";
            if (SpriteCache.TryGetValue(key, out Sprite cached))
            {
                return cached;
            }

            var pixels = new Color32[size * size];
            Vector2[] points = new Vector2[10];
            float outer = size * 0.47f;
            float inner = outer * 0.43f;
            Vector2 center = new Vector2(size * 0.5f, size * 0.52f);
            for (int i = 0; i < 10; i++)
            {
                float angle = Mathf.PI * 0.5f + i * Mathf.PI / 5f;
                float radius = (i & 1) == 0 ? outer : inner;
                points[i] = center + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
            }

            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    bool inside = PointInPolygon(new Vector2(x + 0.5f, y + 0.5f), points);
                    pixels[y * size + x] = inside
                        ? new Color32(255, 255, 255, 255)
                        : new Color32(255, 255, 255, 0);
                }
            }

            Sprite sprite = Sprite.Create(FromPixels(pixels, size, size, "UI_Star"), new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
            SpriteCache[key] = sprite;
            return sprite;
        }

        public static Texture2D ParkingDecal(int width = 256, int height = 144)
        {
            const string key = "parking_decal";
            if (TextureCache.TryGetValue(key, out Texture2D cached))
            {
                return cached;
            }

            var pixels = new Color32[width * height];
            Color32 fill = new Color32(16, 185, 129, 82);
            Color32 border = new Color32(52, 211, 153, 255);
            Fill(pixels, width, height, fill);

            const int dash = 18;
            const int gap = 10;
            for (int x = 0; x < width; x += dash + gap)
            {
                FillRect(pixels, width, height, x, 4, Mathf.Min(dash, width - x), 6, border);
                FillRect(pixels, width, height, x, height - 10, Mathf.Min(dash, width - x), 6, border);
            }

            for (int y = 0; y < height; y += dash + gap)
            {
                FillRect(pixels, width, height, 4, y, 6, Mathf.Min(dash, height - y), border);
                FillRect(pixels, width, height, width - 10, y, 6, Mathf.Min(dash, height - y), border);
            }

            int cx = width / 2;
            int cy = height / 2;
            Color32 white = new Color32(255, 255, 255, 220);
            FillRect(pixels, width, height, cx - 30, cy - 38, 16, 78, white);
            FillCircle(pixels, width, height, cx + 2, cy + 20, 28, white);
            FillCircle(pixels, width, height, cx + 2, cy + 20, 15, new Color32(16, 185, 129, 0));

            Texture2D texture = FromPixels(pixels, width, height, "ParkingDecal");
            TextureCache[key] = texture;
            return texture;
        }

        public static Texture2D Noise(int size, Color baseColor, float variation, int seed)
        {
            string key = $"noise_{size}_{ColorUtility.ToHtmlStringRGB(baseColor)}_{variation:0.00}_{seed}";
            if (TextureCache.TryGetValue(key, out Texture2D cached))
            {
                return cached;
            }

            var random = new System.Random(seed);
            var pixels = new Color32[size * size];
            for (int i = 0; i < pixels.Length; i++)
            {
                float delta = ((float)random.NextDouble() * 2f - 1f) * variation;
                pixels[i] = new Color(
                    Mathf.Clamp01(baseColor.r + delta),
                    Mathf.Clamp01(baseColor.g + delta),
                    Mathf.Clamp01(baseColor.b + delta),
                    1f);
            }

            Texture2D texture = FromPixels(pixels, size, size, "Noise_" + key);
            texture.wrapMode = TextureWrapMode.Repeat;
            texture.filterMode = FilterMode.Bilinear;
            TextureCache[key] = texture;
            return texture;
        }

        public static Texture2D Sign(string kind)
        {
            string key = "sign_" + (kind ?? "default");
            if (TextureCache.TryGetValue(key, out Texture2D cached))
            {
                return cached;
            }

            const int size = 128;
            var pixels = new Color32[size * size];
            Color32 white = new Color32(245, 245, 238, 255);
            Color32 red = new Color32(228, 80, 92, 255);
            Color32 blue = new Color32(28, 143, 195, 255);
            Color32 yellow = new Color32(246, 201, 40, 255);
            Color32 black = new Color32(30, 33, 36, 255);

            switch (kind)
            {
                case "p":
                    FillRect(pixels, size, size, 8, 8, 112, 112, blue);
                    FillRect(pixels, size, size, 46, 28, 14, 72, white);
                    FillCircle(pixels, size, size, 66, 78, 24, white);
                    FillCircle(pixels, size, size, 66, 78, 12, blue);
                    break;
                case "stop":
                    FillCircle(pixels, size, size, 64, 64, 60, white);
                    FillCircle(pixels, size, size, 64, 64, 52, red);
                    FillRect(pixels, size, size, 30, 58, 68, 12, white);
                    break;
                case "work":
                    FillTriangle(pixels, size, size, new Vector2(64, 112), new Vector2(8, 18), new Vector2(120, 18), red);
                    FillTriangle(pixels, size, size, new Vector2(64, 98), new Vector2(22, 28), new Vector2(106, 28), yellow);
                    FillRect(pixels, size, size, 58, 40, 12, 36, black);
                    FillRect(pixels, size, size, 58, 30, 12, 8, black);
                    break;
                case "ped":
                    FillRect(pixels, size, size, 8, 8, 112, 112, blue);
                    FillCircle(pixels, size, size, 64, 92, 10, white);
                    FillRect(pixels, size, size, 59, 58, 10, 26, white);
                    FillRect(pixels, size, size, 44, 42, 10, 22, white);
                    FillRect(pixels, size, size, 74, 42, 10, 22, white);
                    FillRect(pixels, size, size, 40, 66, 20, 8, white);
                    break;
                case "oneway":
                    FillRect(pixels, size, size, 8, 8, 112, 112, black);
                    FillRect(pixels, size, size, 26, 58, 54, 12, white);
                    FillTriangle(pixels, size, size, new Vector2(106, 64), new Vector2(68, 90), new Vector2(68, 38), white);
                    break;
                case "nopark":
                    FillCircle(pixels, size, size, 64, 64, 58, red);
                    FillCircle(pixels, size, size, 64, 64, 48, blue);
                    DrawLine(pixels, size, size, 30, 98, 98, 30, red, 11);
                    break;
                default:
                    FillTriangle(pixels, size, size, new Vector2(64, 116), new Vector2(6, 12), new Vector2(122, 12), yellow);
                    FillRect(pixels, size, size, 58, 36, 12, 46, black);
                    FillRect(pixels, size, size, 58, 22, 12, 10, black);
                    break;
            }

            Texture2D texture = FromPixels(pixels, size, size, "Sign_" + key);
            TextureCache[key] = texture;
            return texture;
        }

        private static Texture2D FromPixels(Color32[] pixels, int width, int height, string name)
        {
            var texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
            {
                name = name,
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            texture.SetPixels32(pixels);
            texture.Apply(false, false);
            return texture;
        }

        private static bool IsInsideRoundedRect(int x, int y, int width, int height, int radius)
        {
            int cx = x < radius ? radius : x >= width - radius ? width - radius - 1 : x;
            int cy = y < radius ? radius : y >= height - radius ? height - radius - 1 : y;
            int dx = x - cx;
            int dy = y - cy;
            return dx * dx + dy * dy <= radius * radius;
        }

        private static void Fill(Color32[] pixels, int width, int height, Color32 color)
        {
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }
        }

        private static void FillRect(Color32[] pixels, int width, int height, int x, int y, int rectWidth, int rectHeight, Color32 color)
        {
            int x0 = Mathf.Max(0, x);
            int y0 = Mathf.Max(0, y);
            int x1 = Mathf.Min(width, x + rectWidth);
            int y1 = Mathf.Min(height, y + rectHeight);
            for (int py = y0; py < y1; py++)
            {
                int row = py * width;
                for (int px = x0; px < x1; px++)
                {
                    pixels[row + px] = color;
                }
            }
        }

        private static void FillCircle(Color32[] pixels, int width, int height, int cx, int cy, int radius, Color32 color)
        {
            int r2 = radius * radius;
            int x0 = Mathf.Max(0, cx - radius);
            int x1 = Mathf.Min(width - 1, cx + radius);
            int y0 = Mathf.Max(0, cy - radius);
            int y1 = Mathf.Min(height - 1, cy + radius);
            for (int y = y0; y <= y1; y++)
            {
                int dy = y - cy;
                for (int x = x0; x <= x1; x++)
                {
                    int dx = x - cx;
                    if (dx * dx + dy * dy <= r2)
                    {
                        pixels[y * width + x] = color;
                    }
                }
            }
        }

        private static void FillTriangle(Color32[] pixels, int width, int height, Vector2 a, Vector2 b, Vector2 c, Color32 color)
        {
            int minX = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(a.x, Mathf.Min(b.x, c.x))));
            int maxX = Mathf.Min(width - 1, Mathf.CeilToInt(Mathf.Max(a.x, Mathf.Max(b.x, c.x))));
            int minY = Mathf.Max(0, Mathf.FloorToInt(Mathf.Min(a.y, Mathf.Min(b.y, c.y))));
            int maxY = Mathf.Min(height - 1, Mathf.CeilToInt(Mathf.Max(a.y, Mathf.Max(b.y, c.y))));
            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    if (PointInTriangle(new Vector2(x + 0.5f, y + 0.5f), a, b, c))
                    {
                        pixels[y * width + x] = color;
                    }
                }
            }
        }

        private static void DrawLine(Color32[] pixels, int width, int height, int x0, int y0, int x1, int y1, Color32 color, int thickness)
        {
            int dx = Mathf.Abs(x1 - x0);
            int dy = Mathf.Abs(y1 - y0);
            int steps = Mathf.Max(dx, dy);
            for (int i = 0; i <= steps; i++)
            {
                float t = steps == 0 ? 0f : (float)i / steps;
                int x = Mathf.RoundToInt(Mathf.Lerp(x0, x1, t));
                int y = Mathf.RoundToInt(Mathf.Lerp(y0, y1, t));
                FillCircle(pixels, width, height, x, y, thickness / 2, color);
            }
        }

        private static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
        {
            float d1 = Sign(p, a, b);
            float d2 = Sign(p, b, c);
            float d3 = Sign(p, c, a);
            bool hasNegative = d1 < 0f || d2 < 0f || d3 < 0f;
            bool hasPositive = d1 > 0f || d2 > 0f || d3 > 0f;
            return !(hasNegative && hasPositive);
        }

        private static float Sign(Vector2 p1, Vector2 p2, Vector2 p3) =>
            (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);

        private static bool PointInPolygon(Vector2 point, Vector2[] polygon)
        {
            bool inside = false;
            for (int i = 0, j = polygon.Length - 1; i < polygon.Length; j = i++)
            {
                if (polygon[i].y > point.y != polygon[j].y > point.y &&
                    point.x < (polygon[j].x - polygon[i].x) * (point.y - polygon[i].y) /
                    (polygon[j].y - polygon[i].y) + polygon[i].x)
                {
                    inside = !inside;
                }
            }

            return inside;
        }
    }
}
