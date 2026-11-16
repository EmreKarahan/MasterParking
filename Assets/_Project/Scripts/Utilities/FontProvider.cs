using UnityEngine;

namespace MasterParking.Utilities
{
    /// <summary>
    /// Resolves a usable UI font without importing a font asset. Unity 2022+
    /// ships LegacyRuntime.ttf; older players may only expose OS fonts.
    /// </summary>
    public static class FontProvider
    {
        private static Font _font;

        public static Font Get()
        {
            if (_font != null)
            {
                return _font;
            }

            _font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            if (_font == null)
            {
                _font = Font.CreateDynamicFontFromOSFont(
                    new[] { "Arial", "Roboto", "Helvetica", "Noto Sans", "Segoe UI" }, 28);
            }

            if (_font == null)
            {
                _font = Font.CreateDynamicFontFromOSFont("Arial", 28);
            }

            return _font;
        }
    }
}
