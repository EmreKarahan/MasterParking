using UnityEngine;
using UnityEngine.Rendering;

namespace MasterParking.Utilities
{
    /// <summary>
    /// Creates materials against whichever render pipeline is active. The project
    /// ships without URP, but this keeps the runtime art compatible if the team
    /// upgrades to URP for a larger production.
    /// </summary>
    public static class MaterialLibrary
    {
        private static Shader _litShader;
        private static Shader _unlitShader;
        private static Shader _decalShader;

        public static Shader LitShader
        {
            get
            {
                if (_litShader == null)
                {
                    _litShader = Shader.Find("Universal Render Pipeline/Lit")
                                 ?? Shader.Find("Standard")
                                 ?? Shader.Find("Legacy Shaders/Diffuse")
                                 ?? Shader.Find("Sprites/Default");
                }

                return _litShader;
            }
        }

        public static Shader UnlitShader
        {
            get
            {
                if (_unlitShader == null)
                {
                    _unlitShader = Shader.Find("Universal Render Pipeline/Unlit")
                                   ?? Shader.Find("Unlit/Color")
                                   ?? Shader.Find("Sprites/Default");
                }

                return _unlitShader;
            }
        }

        public static Shader DecalShader
        {
            get
            {
                if (_decalShader == null)
                {
                    _decalShader = Shader.Find("Sprites/Default")
                                   ?? Shader.Find("Unlit/Transparent")
                                   ?? UnlitShader;
                }

                return _decalShader;
            }
        }

        public static Material CreateLit(Color color, float smoothness = 0.2f, float metallic = 0f, string name = "Lit")
        {
            var material = new Material(LitShader) { name = name };
            SetColor(material, color);
            SetFloat(material, "_Smoothness", smoothness);
            SetFloat(material, "_Glossiness", smoothness);
            SetFloat(material, "_Metallic", metallic);
            material.enableInstancing = true;
            return material;
        }

        public static Material CreateLit(Texture2D texture, Color tint, float smoothness = 0.2f, float metallic = 0f, string name = "LitTextured")
        {
            Material material = CreateLit(tint, smoothness, metallic, name);
            SetTexture(material, texture);
            return material;
        }

        public static Material CreateUnlit(Color color, string name = "Unlit")
        {
            var material = new Material(UnlitShader) { name = name };
            SetColor(material, color);
            return material;
        }

        public static Material CreateUnlit(Texture2D texture, Color tint, string name = "UnlitTextured")
        {
            Material material = CreateUnlit(tint, name);
            SetTexture(material, texture);
            return material;
        }

        public static Material CreateDecal(Texture2D texture)
        {
            var material = new Material(DecalShader) { name = "Decal" };
            SetTexture(material, texture);
            SetColor(material, Color.white);
            if (material.HasProperty("_Surface"))
            {
                material.SetFloat("_Surface", 1f);
                material.SetFloat("_Blend", 0f);
            }

            material.renderQueue = (int)RenderQueue.Transparent;
            return material;
        }

        public static void SetColor(Material material, Color color)
        {
            if (material == null)
            {
                return;
            }

            if (material.HasProperty("_BaseColor"))
            {
                material.SetColor("_BaseColor", color);
            }

            if (material.HasProperty("_Color"))
            {
                material.SetColor("_Color", color);
            }
        }

        public static void SetTexture(Material material, Texture2D texture)
        {
            if (material == null || texture == null)
            {
                return;
            }

            if (material.HasProperty("_BaseMap"))
            {
                material.SetTexture("_BaseMap", texture);
            }

            if (material.HasProperty("_MainTex"))
            {
                material.SetTexture("_MainTex", texture);
            }
        }

        public static void SetFloat(Material material, string property, float value)
        {
            if (material != null && material.HasProperty(property))
            {
                material.SetFloat(property, value);
            }
        }

        public static void SetTiling(Material material, float x, float y)
        {
            if (material == null)
            {
                return;
            }

            if (material.HasProperty("_BaseMap"))
            {
                material.SetTextureScale("_BaseMap", new Vector2(x, y));
            }

            if (material.HasProperty("_MainTex"))
            {
                material.SetTextureScale("_MainTex", new Vector2(x, y));
            }
        }

        public static void SetShadowMode(Renderer renderer, bool cast, bool receive)
        {
            if (renderer == null)
            {
                return;
            }

            renderer.shadowCastingMode = cast ? ShadowCastingMode.On : ShadowCastingMode.Off;
            renderer.receiveShadows = receive;
        }
    }
}
