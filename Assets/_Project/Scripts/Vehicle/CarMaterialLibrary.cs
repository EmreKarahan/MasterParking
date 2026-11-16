using System.Collections.Generic;
using MasterParking.Utilities;
using UnityEngine;
using UnityEngine.Rendering;

namespace MasterParking.Vehicle
{
    /// <summary>
    /// Shared materials for player and parked cars. The palette atlas from the
    /// original model is used directly; tints only nudge parked-car variety.
    /// </summary>
    public static class CarMaterialLibrary
    {
        private const string PaletteResourcePath = "Vehicles/VehiclePalette";

        private static readonly Dictionary<string, Material> BodyMaterials = new Dictionary<string, Material>();
        private static Texture2D _palette;
        private static Material _glass;
        private static Material _placeholder;

        public static Material Body(Color tint)
        {
            string key = ColorUtility.ToHtmlStringRGB(tint);
            if (BodyMaterials.TryGetValue(key, out Material cached))
            {
                return cached;
            }

            Material material = MaterialLibrary.CreateLit(
                Palette(), tint, 0.45f, 0.05f, "CarBody_" + key);
            BodyMaterials[key] = material;
            return material;
        }

        public static Material Glass()
        {
            if (_glass != null)
            {
                return _glass;
            }

            _glass = MaterialLibrary.CreateLit(Palette(), new Color(0.78f, 0.9f, 1f, 0.62f), 0.85f, 0.05f, "CarGlass");
            MakeTransparent(_glass);
            return _glass;
        }

        public static Material Placeholder(Color tint)
        {
            if (_placeholder == null)
            {
                _placeholder = MaterialLibrary.CreateLit(tint, 0.35f, 0.05f, "CarPlaceholder");
            }

            return _placeholder;
        }

        public static Texture2D Palette()
        {
            if (_palette == null)
            {
                _palette = Resources.Load<Texture2D>(PaletteResourcePath);
            }

            return _palette;
        }

        private static void MakeTransparent(Material material)
        {
            if (material == null)
            {
                return;
            }

            if (material.HasProperty("_Mode"))
            {
                material.SetFloat("_Mode", 3f);
                material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.EnableKeyword("_ALPHABLEND_ON");
                material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                material.renderQueue = (int)RenderQueue.Transparent;
            }

            if (material.HasProperty("_Surface"))
            {
                material.SetFloat("_Surface", 1f);
                material.SetFloat("_Blend", 0f);
                material.SetFloat("_ZWrite", 0f);
                material.renderQueue = (int)RenderQueue.Transparent;
            }
        }
    }
}
