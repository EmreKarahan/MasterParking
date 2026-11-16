using UnityEngine;

namespace MasterParking.Utilities
{
    /// <summary>Colour helpers shared by runtime-built art and UI.</summary>
    public static class ColorUtil
    {
        public static readonly Color Transparent = new Color(0f, 0f, 0f, 0f);

        public static Color Hex(string hex, Color fallback)
        {
            if (string.IsNullOrWhiteSpace(hex))
            {
                return fallback;
            }

            return ColorUtility.TryParseHtmlString(hex, out Color parsed) ? parsed : fallback;
        }

        public static Color Hex(string hex) => Hex(hex, Color.white);

        public static Color WithAlpha(Color color, float alpha)
        {
            color.a = alpha;
            return color;
        }

        public static Color Lighten(Color color, float amount) =>
            new Color(
                Mathf.Clamp01(color.r + amount),
                Mathf.Clamp01(color.g + amount),
                Mathf.Clamp01(color.b + amount),
                color.a);

        public static Color Darken(Color color, float amount) =>
            new Color(
                Mathf.Clamp01(color.r - amount),
                Mathf.Clamp01(color.g - amount),
                Mathf.Clamp01(color.b - amount),
                color.a);
    }
}
