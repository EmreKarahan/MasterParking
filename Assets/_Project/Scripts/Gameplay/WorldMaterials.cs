using MasterParking.Utilities;
using UnityEngine;

namespace MasterParking.Gameplay
{
    /// <summary>
    /// Shared world materials. Cached per process so building and rebuilding a
    /// level does not leak dozens of Material instances.
    /// </summary>
    public static class WorldMaterials
    {
        private static Material _asphalt;
        private static Material _paint;
        private static Material _wall;
        private static Material _wallStripe;
        private static Material _cone;
        private static Material _foliage;
        private static Material _trunk;
        private static Material _metal;
        private static Material _lamp;
        private static Material _marker;
        private static Material _markerDecal;
        private static Material _dark;
        private static Material _lightRed;
        private static Material _lightYellow;
        private static Material _lightGreen;
        private static Material _lightOff;

        public static Material Asphalt
        {
            get
            {
                if (_asphalt == null)
                {
                    Texture2D noise = TextureFactory.Noise(64, new Color(0.19f, 0.22f, 0.27f), 0.035f, 11);
                    _asphalt = MaterialLibrary.CreateLit(noise, Color.white, 0.08f, 0f, "Asphalt");
                    MaterialLibrary.SetTiling(_asphalt, 18f, 12f);
                }

                return _asphalt;
            }
        }

        public static Material Paint => _paint ??= MaterialLibrary.CreateUnlit(new Color(0.93f, 0.95f, 0.96f), "RoadPaint");

        public static Material Wall => _wall ??= MaterialLibrary.CreateLit(new Color(0.30f, 0.34f, 0.40f), 0.12f, 0.05f, "Wall");

        public static Material WallStripe => _wallStripe ??= MaterialLibrary.CreateLit(new Color(0.93f, 0.72f, 0.2f), 0.2f, 0f, "WallStripe");

        public static Material Cone => _cone ??= MaterialLibrary.CreateLit(new Color(0.95f, 0.44f, 0.14f), 0.25f, 0f, "Cone");

        public static Material Foliage => _foliage ??= MaterialLibrary.CreateLit(new Color(0.16f, 0.45f, 0.28f), 0.15f, 0f, "Foliage");

        public static Material Trunk => _trunk ??= MaterialLibrary.CreateLit(new Color(0.32f, 0.22f, 0.14f), 0.1f, 0f, "Trunk");

        public static Material Metal => _metal ??= MaterialLibrary.CreateLit(new Color(0.62f, 0.66f, 0.7f), 0.45f, 0.3f, "Metal");

        public static Material Lamp => _lamp ??= MaterialLibrary.CreateUnlit(new Color(1f, 0.87f, 0.55f), "Lamp");

        public static Material Marker => _marker ??= MaterialLibrary.CreateUnlit(new Color(0.2f, 0.95f, 0.65f), "Marker");

        public static Material Dark => _dark ??= MaterialLibrary.CreateLit(new Color(0.11f, 0.13f, 0.16f), 0.3f, 0.1f, "DarkMetal");

        public static Material LightRed => _lightRed ??= MaterialLibrary.CreateUnlit(new Color(0.9f, 0.18f, 0.2f), "LightRed");

        public static Material LightYellow => _lightYellow ??= MaterialLibrary.CreateUnlit(new Color(0.95f, 0.75f, 0.16f), "LightYellow");

        public static Material LightGreen => _lightGreen ??= MaterialLibrary.CreateUnlit(new Color(0.2f, 0.85f, 0.42f), "LightGreen");

        public static Material LightOff => _lightOff ??= MaterialLibrary.CreateLit(new Color(0.16f, 0.18f, 0.2f), 0.2f, 0f, "LightOff");

        public static Material ParkingDecal
        {
            get
            {
                if (_markerDecal == null)
                {
                    _markerDecal = MaterialLibrary.CreateDecal(TextureFactory.ParkingDecal());
                }

                return _markerDecal;
            }
        }

        private static readonly System.Collections.Generic.Dictionary<string, Material> SignMaterials =
            new System.Collections.Generic.Dictionary<string, Material>();

        public static Material SignMaterial(string kind)
        {
            string key = string.IsNullOrEmpty(kind) ? "warn" : kind;
            if (!SignMaterials.TryGetValue(key, out Material material) || material == null)
            {
                material = MaterialLibrary.CreateDecal(TextureFactory.Sign(key));
                SignMaterials[key] = material;
            }

            return material;
        }
    }
}
