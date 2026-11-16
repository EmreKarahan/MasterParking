// Compile-time stubs only. This file is never included in a Unity build.
// It mirrors the small slice of the Unity API used by MasterParking so the
// runtime scripts can be type-checked with dotnet on machines without Unity.
using System;
using System.Collections.Generic;

namespace UnityEngine
{
    public class Object
    {
        public string name = string.Empty;

        public static void Destroy(Object target) { }
        public static void Destroy(Object target, float delay) { }
        public static void DontDestroyOnLoad(Object target) { }
        public static T FindAnyObjectByType<T>() where T : Object => null;
        public static implicit operator bool(Object value) => value != null;
        public static bool operator ==(Object a, Object b) => ReferenceEquals(a, b);
        public static bool operator !=(Object a, Object b) => !ReferenceEquals(a, b);
        public override bool Equals(object other) => ReferenceEquals(this, other);
        public override int GetHashCode() => base.GetHashCode();
    }

    public static class Mathf
    {
        public const float PI = (float)Math.PI;
        public const float Deg2Rad = PI / 180f;
        public const float Rad2Deg = 180f / PI;

        public static float Clamp(float value, float min, float max) => Math.Min(max, Math.Max(min, value));
        public static int Clamp(int value, int min, int max) => Math.Min(max, Math.Max(min, value));
        public static float Clamp01(float value) => Clamp(value, 0f, 1f);
        public static float MoveTowards(float current, float target, float maxDelta) => Math.Abs(target - current) <= maxDelta ? target : current + Math.Sign(target - current) * maxDelta;
        public static float Abs(float value) => Math.Abs(value);
        public static int Abs(int value) => Math.Abs(value);
        public static float Min(float a, float b) => Math.Min(a, b);
        public static int Min(int a, int b) => Math.Min(a, b);
        public static float Max(float a, float b) => Math.Max(a, b);
        public static int Max(int a, int b) => Math.Max(a, b);
        public static int RoundToInt(float value) => (int)Math.Round(value);
        public static int FloorToInt(float value) => (int)Math.Floor(value);
        public static int CeilToInt(float value) => (int)Math.Ceiling(value);
        public static float Round(float value) => (float)Math.Round(value);
        public static float Sin(float value) => (float)Math.Sin(value);
        public static float Cos(float value) => (float)Math.Cos(value);
        public static float Tan(float value) => (float)Math.Tan(value);
        public static float Atan2(float y, float x) => (float)Math.Atan2(y, x);
        public static float Exp(float value) => (float)Math.Exp(value);
        public static float Sqrt(float value) => (float)Math.Sqrt(value);
        public static float Pow(float value, float power) => (float)Math.Pow(value, power);
        public static float Lerp(float a, float b, float t) => a + (b - a) * Clamp01(t);
        public static float InverseLerp(float a, float b, float value) => a == b ? 0f : Clamp01((value - a) / (b - a));
        public static float DeltaAngle(float current, float target)
        {
            float delta = (target - current) % 360f;
            if (delta < -180f) delta += 360f;
            if (delta > 180f) delta -= 360f;
            return delta;
        }
        public static float Repeat(float t, float length) => t - (float)Math.Floor(t / length) * length;
    }

    public struct Vector2
    {
        public float x;
        public float y;

        public Vector2(float x, float y) { this.x = x; this.y = y; }
        public static Vector2 zero => new Vector2(0f, 0f);
        public static Vector2 one => new Vector2(1f, 1f);
        public float magnitude => (float)Math.Sqrt(x * x + y * y);
        public float sqrMagnitude => x * x + y * y;
        public Vector2 normalized { get { float m = magnitude; return m > 1e-6f ? new Vector2(x / m, y / m) : zero; } }

        public static float Distance(Vector2 a, Vector2 b) => (a - b).magnitude;
        public static Vector2 Lerp(Vector2 a, Vector2 b, float t) => a + (b - a) * Mathf.Clamp01(t);
        public static Vector2 operator +(Vector2 a, Vector2 b) => new Vector2(a.x + b.x, a.y + b.y);
        public static Vector2 operator -(Vector2 a, Vector2 b) => new Vector2(a.x - b.x, a.y - b.y);
        public static Vector2 operator -(Vector2 a) => new Vector2(-a.x, -a.y);
        public static Vector2 operator *(Vector2 a, float d) => new Vector2(a.x * d, a.y * d);
        public static Vector2 operator *(float d, Vector2 a) => new Vector2(a.x * d, a.y * d);
        public static Vector2 operator /(Vector2 a, float d) => new Vector2(a.x / d, a.y / d);
        public static bool operator ==(Vector2 a, Vector2 b) => a.x == b.x && a.y == b.y;
        public static bool operator !=(Vector2 a, Vector2 b) => !(a == b);
        public override bool Equals(object other) => other is Vector2 v && this == v;
        public override int GetHashCode() => x.GetHashCode() ^ (y.GetHashCode() << 2);
        public override string ToString() => $"({x}, {y})";
    }

    public struct Vector3
    {
        public float x;
        public float y;
        public float z;

        public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
        public Vector3(float x, float y) { this.x = x; this.y = y; z = 0f; }
        public static Vector3 zero => new Vector3(0f, 0f, 0f);
        public static Vector3 one => new Vector3(1f, 1f, 1f);
        public static Vector3 up => new Vector3(0f, 1f, 0f);
        public static Vector3 down => new Vector3(0f, -1f, 0f);
        public static Vector3 right => new Vector3(1f, 0f, 0f);
        public static Vector3 left => new Vector3(-1f, 0f, 0f);
        public static Vector3 forward => new Vector3(0f, 0f, 1f);
        public static Vector3 back => new Vector3(0f, 0f, -1f);
        public float magnitude => (float)Math.Sqrt(x * x + y * y + z * z);
        public float sqrMagnitude => x * x + y * y + z * z;
        public Vector3 normalized { get { float m = magnitude; return m > 1e-6f ? new Vector3(x / m, y / m, z / m) : zero; } }

        public static float Distance(Vector3 a, Vector3 b) => (a - b).magnitude;
        public static float Dot(Vector3 a, Vector3 b) => a.x * b.x + a.y * b.y + a.z * b.z;
        public static Vector3 Cross(Vector3 a, Vector3 b) => new Vector3(a.y * b.z - a.z * b.y, a.z * b.x - a.x * b.z, a.x * b.y - a.y * b.x);
        public static Vector3 Lerp(Vector3 a, Vector3 b, float t) => a + (b - a) * Mathf.Clamp01(t);
        public static Vector3 operator +(Vector3 a, Vector3 b) => new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(a.x - b.x, a.y - b.y, a.z - b.z);
        public static Vector3 operator -(Vector3 a) => new Vector3(-a.x, -a.y, -a.z);
        public static Vector3 operator *(Vector3 a, float d) => new Vector3(a.x * d, a.y * d, a.z * d);
        public static Vector3 operator *(float d, Vector3 a) => new Vector3(a.x * d, a.y * d, a.z * d);
        public static Vector3 operator /(Vector3 a, float d) => new Vector3(a.x / d, a.y / d, a.z / d);
        public static bool operator ==(Vector3 a, Vector3 b) => a.x == b.x && a.y == b.y && a.z == b.z;
        public static bool operator !=(Vector3 a, Vector3 b) => !(a == b);
        public override bool Equals(object other) => other is Vector3 v && this == v;
        public override int GetHashCode() => x.GetHashCode() ^ (y.GetHashCode() << 2) ^ (z.GetHashCode() >> 2);
        public override string ToString() => $"({x}, {y}, {z})";
    }

    public struct Vector4
    {
        public float x;
        public float y;
        public float z;
        public float w;
        public Vector4(float x, float y, float z, float w) { this.x = x; this.y = y; this.z = z; this.w = w; }
    }

    public struct Quaternion
    {
        public float x;
        public float y;
        public float z;
        public float w;

        public Quaternion(float x, float y, float z, float w) { this.x = x; this.y = y; this.z = z; this.w = w; }
        public static Quaternion identity => new Quaternion(0f, 0f, 0f, 1f);
        public static Quaternion Euler(float x, float y, float z) => identity;
        public static Quaternion Euler(Vector3 euler) => identity;
        public static Quaternion LookRotation(Vector3 forward, Vector3 upwards) => identity;
        public static Quaternion LookRotation(Vector3 forward) => identity;
        public static Quaternion Slerp(Quaternion a, Quaternion b, float t) => t < 0.5f ? a : b;
        public static Quaternion operator *(Quaternion a, Quaternion b) => a;
        public static Vector3 operator *(Quaternion rotation, Vector3 point) => point;
        public static bool operator ==(Quaternion a, Quaternion b) => a.x == b.x && a.y == b.y && a.z == b.z && a.w == b.w;
        public static bool operator !=(Quaternion a, Quaternion b) => !(a == b);
        public override bool Equals(object other) => other is Quaternion q && this == q;
        public override int GetHashCode() => x.GetHashCode() ^ y.GetHashCode() ^ z.GetHashCode() ^ w.GetHashCode();
    }

    public struct Color
    {
        public float r;
        public float g;
        public float b;
        public float a;

        public Color(float r, float g, float b, float a = 1f) { this.r = r; this.g = g; this.b = b; this.a = a; }
        public static Color white => new Color(1f, 1f, 1f, 1f);
        public static Color black => new Color(0f, 0f, 0f, 1f);
        public static Color clear => new Color(0f, 0f, 0f, 0f);
        public static Color Lerp(Color a, Color b, float t)
        {
            t = Mathf.Clamp01(t);
            return new Color(a.r + (b.r - a.r) * t, a.g + (b.g - a.g) * t, a.b + (b.b - a.b) * t, a.a + (b.a - a.a) * t);
        }
        public static Color operator *(Color a, Color b) => new Color(a.r * b.r, a.g * b.g, a.b * b.b, a.a * b.a);
        public static Color operator *(Color a, float d) => new Color(a.r * d, a.g * d, a.b * d, a.a * d);
        public static implicit operator Color32(Color color) => new Color32(
            (byte)Mathf.Clamp(Mathf.RoundToInt(color.r * 255f), 0, 255),
            (byte)Mathf.Clamp(Mathf.RoundToInt(color.g * 255f), 0, 255),
            (byte)Mathf.Clamp(Mathf.RoundToInt(color.b * 255f), 0, 255),
            (byte)Mathf.Clamp(Mathf.RoundToInt(color.a * 255f), 0, 255));
    }

    public struct Color32
    {
        public byte r;
        public byte g;
        public byte b;
        public byte a;
        public Color32(byte r, byte g, byte b, byte a) { this.r = r; this.g = g; this.b = b; this.a = a; }
        public static implicit operator Color(Color32 color) => new Color(color.r / 255f, color.g / 255f, color.b / 255f, color.a / 255f);
    }

    public struct Rect
    {
        public float x;
        public float y;
        public float width;
        public float height;

        public Rect(float x, float y, float width, float height) { this.x = x; this.y = y; this.width = width; this.height = height; }
        public Vector2 position { get => new Vector2(x, y); set { x = value.x; y = value.y; } }
        public Vector2 size { get => new Vector2(width, height); set { width = value.x; height = value.y; } }
        public float xMin => x;
        public float yMin => y;
        public float xMax => x + width;
        public float yMax => y + height;
        public static bool operator ==(Rect a, Rect b) => a.x == b.x && a.y == b.y && a.width == b.width && a.height == b.height;
        public static bool operator !=(Rect a, Rect b) => !(a == b);
        public override bool Equals(object other) => other is Rect r && this == r;
        public override int GetHashCode() => x.GetHashCode() ^ y.GetHashCode() ^ width.GetHashCode() ^ height.GetHashCode();
    }

    public struct Bounds
    {
        public Vector3 center;
        public Vector3 size;
        public Bounds(Vector3 center, Vector3 size) { this.center = center; this.size = size; }
        public Vector3 extents => size * 0.5f;
    }

    public struct Matrix4x4
    {
        public static Matrix4x4 identity => default;
    }

    public static class ColorUtility
    {
        public static bool TryParseHtmlString(string html, out Color color)
        {
            color = Color.white;
            if (string.IsNullOrEmpty(html)) return false;
            string value = html.TrimStart('#');
            if (value.Length != 6 && value.Length != 8) return false;
            if (!uint.TryParse(value, System.Globalization.NumberStyles.HexNumber, null, out uint packed)) return false;
            if (value.Length == 6)
            {
                color = new Color(((packed >> 16) & 0xFF) / 255f, ((packed >> 8) & 0xFF) / 255f, (packed & 0xFF) / 255f, 1f);
            }
            else
            {
                color = new Color(((packed >> 24) & 0xFF) / 255f, ((packed >> 16) & 0xFF) / 255f, ((packed >> 8) & 0xFF) / 255f, (packed & 0xFF) / 255f);
            }
            return true;
        }

        public static string ToHtmlStringRGB(Color color) =>
            $"{(byte)(color.r * 255f):X2}{(byte)(color.g * 255f):X2}{(byte)(color.b * 255f):X2}";
    }

    public class Component : Object
    {
        public GameObject gameObject { get; internal set; }
        public Transform transform => gameObject != null ? gameObject.transform : null;

        public T GetComponent<T>() where T : class => gameObject?.GetComponent<T>();
        public T GetComponentInChildren<T>() where T : class => gameObject?.GetComponentInChildren<T>();
        public T AddComponent<T>() where T : Component => gameObject?.AddComponent<T>();
    }

    public class Behaviour : Component
    {
        public bool enabled = true;
    }

    public class MonoBehaviour : Behaviour { }

    public class ScriptableObject : Object
    {
        public static T CreateInstance<T>() where T : ScriptableObject, new() => new T();
    }

    public class Transform : Component
    {
        private readonly List<Transform> _children = new List<Transform>();
        public Transform parent { get; private set; }
        public Vector3 position;
        public Vector3 localPosition;
        public Vector3 localScale = Vector3.one;
        public Quaternion rotation = Quaternion.identity;
        public Quaternion localRotation = Quaternion.identity;

        public Vector3 right => new Vector3(1f, 0f, 0f);
        public Vector3 up => new Vector3(0f, 1f, 0f);
        public Vector3 forward => new Vector3(0f, 0f, 1f);
        public int childCount => _children.Count;

        public void SetParent(Transform newParent, bool worldPositionStays)
        {
            parent?._children.Remove(this);
            parent = newParent;
            newParent?._children.Add(this);
        }

        public Transform GetChild(int index) => _children[index];

        public Transform Find(string childName)
        {
            for (int i = 0; i < _children.Count; i++)
            {
                if (_children[i].gameObject != null && _children[i].gameObject.name == childName) return _children[i];
            }
            return null;
        }

        public void SetPositionAndRotation(Vector3 newPosition, Quaternion newRotation)
        {
            position = newPosition;
            localPosition = newPosition;
            rotation = newRotation;
            localRotation = newRotation;
        }

        public void Rotate(float x, float y, float z, Space space) { }
        public void SetAsLastSibling() { }
        public void SetAsFirstSibling() { }
    }

    public class RectTransform : Transform
    {
        public Vector2 anchorMin;
        public Vector2 anchorMax;
        public Vector2 pivot = new Vector2(0.5f, 0.5f);
        public Vector2 anchoredPosition;
        public Vector2 sizeDelta;
        public Vector2 offsetMin;
        public Vector2 offsetMax;
        public Rect rect => new Rect(0f, 0f, sizeDelta.x, sizeDelta.y);
    }

    public class GameObject : Object
    {
        private readonly List<Component> _components = new List<Component>();

        public GameObject(string name = "GameObject", params Type[] components)
        {
            this.name = name;

            bool wantsRect = false;
            for (int i = 0; i < components.Length; i++)
            {
                if (components[i] == typeof(RectTransform)) wantsRect = true;
            }

            Transform transform = wantsRect ? new RectTransform() : new Transform();
            transform.gameObject = this;
            _components.Add(transform);
            this.transform = transform;

            for (int i = 0; i < components.Length; i++)
            {
                Type type = components[i];
                if (type == typeof(Transform) || type == typeof(RectTransform)) continue;
                AddComponent(type);
            }
        }

        public Transform transform { get; }
        public int layer;
        public bool activeSelf { get; private set; } = true;

        public T AddComponent<T>() where T : Component => (T)AddComponent(typeof(T));

        public Component AddComponent(Type type)
        {
            var component = (Component)Activator.CreateInstance(type);
            component.gameObject = this;
            _components.Add(component);
            return component;
        }

        public T GetComponent<T>() where T : class
        {
            for (int i = 0; i < _components.Count; i++)
            {
                if (_components[i] is T match) return match;
            }
            return null;
        }

        public T GetComponentInChildren<T>() where T : class
        {
            T own = GetComponent<T>();
            if (own != null) return own;
            for (int i = 0; i < transform.childCount; i++)
            {
                T child = transform.GetChild(i).gameObject.GetComponentInChildren<T>();
                if (child != null) return child;
            }
            return null;
        }

        public void SetActive(bool value) => activeSelf = value;

        public static GameObject CreatePrimitive(PrimitiveType type)
        {
            var gameObject = new GameObject(type.ToString());
            gameObject.AddComponent<MeshFilter>();
            gameObject.AddComponent<MeshRenderer>();
            gameObject.AddComponent<BoxCollider>();
            return gameObject;
        }
    }

    public enum PrimitiveType { Sphere, Capsule, Cylinder, Cube, Plane, Quad }
    public enum Space { World, Self }
    public enum TextAnchor { UpperLeft, UpperCenter, UpperRight, MiddleLeft, MiddleCenter, MiddleRight, LowerLeft, LowerCenter, LowerRight }
    public enum FontStyle { Normal, Bold, Italic, BoldAndItalic }
    public enum HorizontalWrapMode { Wrap, Overflow }
    public enum VerticalWrapMode { Truncate, Overflow }
    public enum FilterMode { Point, Bilinear, Trilinear }
    public enum TextureWrapMode { Repeat, Clamp, Mirror, MirrorOnce }
    public enum TextureFormat { Alpha8, RGB24, RGBA32, ARGB32 }
    public enum SpriteMeshType { FullRect, Tight }
    public enum RenderMode { ScreenSpaceOverlay, ScreenSpaceCamera, WorldSpace }
    public enum CameraClearFlags { Skybox, Color, SolidColor, Depth, Nothing }
    public enum LightType { Spot, Directional, Point, Area }
    public enum LightShadows { None, Hard, Soft }
    public enum QueryTriggerInteraction { UseGlobal, Ignore, Collide }
    public static class SleepTimeout
    {
        public const int NeverSleep = -1;
        public const int SystemSetting = -2;
    }

    public class Texture : Object { }

    public class Texture2D : Texture
    {
        public int width { get; private set; }
        public int height { get; private set; }
        public FilterMode filterMode;
        public TextureWrapMode wrapMode;
        public Texture2D(int width, int height, TextureFormat format, bool mipChain) { this.width = width; this.height = height; }
        public void SetPixels32(Color32[] pixels) { }
        public void Apply(bool updateMipmaps, bool makeNoLongerReadable) { }
    }

    public class Sprite : Object
    {
        public Rect rect;
        public Texture2D texture;

        public static Sprite Create(Texture2D texture, Rect rect, Vector2 pivot, float pixelsPerUnit) =>
            new Sprite { texture = texture, rect = rect };

        public static Sprite Create(Texture2D texture, Rect rect, Vector2 pivot, float pixelsPerUnit, uint extrude, SpriteMeshType meshType, Vector4 border) =>
            new Sprite { texture = texture, rect = rect };
    }

    public class Font : Object
    {
        public static Font CreateDynamicFontFromOSFont(string[] fontnames, int size) => new Font();
        public static Font CreateDynamicFontFromOSFont(string fontname, int size) => new Font();
    }

    public class TextAsset : Object
    {
        public string text = string.Empty;
    }

    public class Shader : Object
    {
        public static Shader Find(string name) => new Shader();
    }

    public class Material : Object
    {
        public Material(Shader shader) { }
        public bool enableInstancing;
        public int renderQueue;
        public bool HasProperty(string property) => true;
        public void SetColor(string property, Color color) { }
        public void SetTexture(string property, Texture texture) { }
        public void SetTextureScale(string property, Vector2 scale) { }
        public void SetFloat(string property, float value) { }
        public void SetInt(string property, int value) { }
        public void EnableKeyword(string keyword) { }
        public void DisableKeyword(string keyword) { }
    }

    public class Mesh : Object
    {
        public Rendering.IndexFormat indexFormat;
        public Bounds bounds;
        public void SetVertices(List<Vector3> vertices) { }
        public void SetTriangles(List<int> triangles, int submesh) { }
        public void SetNormals(List<Vector3> normals) { }
        public void SetUVs(int channel, List<Vector2> uvs) { }
        public void RecalculateBounds() { }
        public void RecalculateNormals() { }
        public void CombineMeshes(CombineInstance[] combine, bool mergeSubMeshes, bool useMatrices) { }
    }

    public struct CombineInstance
    {
        public Mesh mesh;
        public Matrix4x4 transform;
    }

    public class MeshFilter : Component
    {
        public Mesh sharedMesh;
    }

    public class Renderer : Component
    {
        public Material sharedMaterial;
        public Rendering.ShadowCastingMode shadowCastingMode;
        public bool receiveShadows;
    }

    public class MeshRenderer : Renderer { }

    public class Collider : Component
    {
        public bool isTrigger;
    }

    public class BoxCollider : Collider
    {
        public Vector3 center;
        public Vector3 size;
    }

    public class MeshCollider : Collider
    {
        public Mesh sharedMesh;
    }

    public static class Physics
    {
        public static int OverlapBoxNonAlloc(Vector3 center, Vector3 halfExtents, Collider[] results, Quaternion orientation, int mask, QueryTriggerInteraction queryTriggerInteraction) => 0;
    }

    public class Camera : Behaviour
    {
        public CameraClearFlags clearFlags;
        public Color backgroundColor;
        public float fieldOfView = 60f;
        public float nearClipPlane = 0.3f;
        public float farClipPlane = 1000f;
        public float aspect = 16f / 9f;
        public int cullingMask;
        public bool orthographic;
        public float orthographicSize = 5f;
        public float depth;
    }

    public class AudioListener : Behaviour { }

    public class AudioClip : Object
    {
        public static AudioClip Create(string name, int lengthSamples, int channels, int frequency, bool stream) => new AudioClip();
        public void SetData(float[] data, int offsetSamples) { }
    }

    public class AudioSource : Behaviour
    {
        public AudioClip clip;
        public bool playOnAwake = true;
        public bool loop;
        public float spatialBlend;
        public float volume = 1f;
        public float pitch = 1f;
        public bool isPlaying { get; private set; }
        public void Play() => isPlaying = true;
        public void Stop() => isPlaying = false;
        public void PlayOneShot(AudioClip clip, float volumeScale) { }
    }

    public class Light : Behaviour
    {
        public LightType type;
        public Color color;
        public float intensity;
        public LightShadows shadows;
        public float shadowStrength;
        public float shadowBias;
        public float shadowNormalBias;
    }

    public static class Resources
    {
        public static T Load<T>(string path) where T : Object => null;
        public static T GetBuiltinResource<T>(string path) where T : Object => null;
    }

    public static class StaticBatchingUtility
    {
        public static void Combine(GameObject staticBatchRoot) { }
        public static void Combine(GameObject[] staticBatchRoots) { }
    }

    public static class QualitySettings
    {
        public static int vSyncCount;
        public static float shadowDistance;
    }

    public static class RenderSettings
    {
        public static Rendering.AmbientMode ambientMode;
        public static Color ambientLight;
        public static bool fog;
        public static Material skybox;
    }

    public static class Screen
    {
        public static int width = 1920;
        public static int height = 1080;
        public static Rect safeArea = new Rect(0f, 0f, 1920f, 1080f);
        public static int sleepTimeout;
        public static ScreenOrientation orientation;
        public static bool autorotateToPortrait;
        public static bool autorotateToPortraitUpsideDown;
        public static bool autorotateToLandscapeLeft;
        public static bool autorotateToLandscapeRight;
    }

    public enum ScreenOrientation
    {
        Portrait,
        PortraitUpsideDown,
        LandscapeLeft,
        LandscapeRight,
        AutoRotation
    }

    public static class Application
    {
        public static string persistentDataPath = System.IO.Path.GetTempPath();
        public static int targetFrameRate;
        public static void Quit() { }
        public static bool isEditor => false;
        public static bool isMobilePlatform => false;
    }

    public static class PlayerPrefs
    {
        private static readonly Dictionary<string, string> Store = new Dictionary<string, string>();
        public static string GetString(string key, string defaultValue = "") =>
            Store.TryGetValue(key, out string value) ? value : defaultValue;
        public static void SetString(string key, string value) => Store[key] = value;
        public static bool HasKey(string key) => Store.ContainsKey(key);
        public static void DeleteKey(string key) => Store.Remove(key);
        public static void Save() { }
    }

    public static class JsonUtility
    {
        public static T FromJson<T>(string json) => default;
        public static string ToJson(object obj, bool prettyPrint = false) => string.Empty;
    }

    public static class Debug
    {
        public static void Log(object message) { }
        public static void LogWarning(object message) { }
        public static void LogError(object message) { }
    }

    public static class Time
    {
        public static float deltaTime = 0.016f;
        public static float time;
        public static float unscaledDeltaTime = 0.016f;
    }

    public static class Random
    {
        public static float value => 0.5f;
        public static Vector3 insideUnitSphere => Vector3.zero;
    }

    public static class LayerMask
    {
        public static int NameToLayer(string layerName) => -1;
    }

    public static class Handheld
    {
        public static void Vibrate() { }
    }

    public enum KeyCode
    {
        None, W, A, S, D, P, R, C, Space, Escape, F2,
        Alpha1, Alpha2, Alpha3,
        UpArrow, DownArrow, LeftArrow, RightArrow
    }

    public static class Input
    {
        public static bool GetKey(KeyCode key) => false;
        public static bool GetKeyDown(KeyCode key) => false;
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class DefaultExecutionOrder : Attribute
    {
        public DefaultExecutionOrder(int order) { }
    }

    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class RequireComponent : Attribute
    {
        public RequireComponent(Type type) { }
    }

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class CreateAssetMenuAttribute : Attribute
    {
        public string menuName { get; set; }
        public string fileName { get; set; }
        public int order { get; set; }
    }
}

namespace UnityEngine.Rendering
{
    public enum IndexFormat { UInt16, UInt32 }
    public enum ShadowCastingMode { Off, On, TwoSided, ShadowsOnly }
    public enum BlendMode
    {
        Zero = 0, One = 1, DstColor = 2, SrcColor = 3,
        OneMinusDstColor = 4, SrcAlpha = 5, OneMinusSrcColor = 6,
        DstAlpha = 7, OneMinusDstAlpha = 8, SrcAlphaSaturate = 9, OneMinusSrcAlpha = 10
    }
    public enum RenderQueue { Background = 1000, Geometry = 2000, AlphaTest = 2450, GeometryLast = 2500, Transparent = 3000, Overlay = 4000 }
    public enum AmbientMode { Skybox = 0, Trilight = 1, Flat = 3, Custom = 4 }
}

namespace UnityEngine.Events
{
    public delegate void UnityAction();
}

namespace UnityEngine.EventSystems
{
    public class BaseEventData { }

    public class PointerEventData : BaseEventData
    {
        public Vector2 position;
        public Vector2 delta;
        public Camera pressEventCamera;
    }

    public interface IEventSystemHandler { }
    public interface IBeginDragHandler : IEventSystemHandler { void OnBeginDrag(PointerEventData eventData); }
    public interface IDragHandler : IEventSystemHandler { void OnDrag(PointerEventData eventData); }
    public interface IEndDragHandler : IEventSystemHandler { void OnEndDrag(PointerEventData eventData); }
    public interface IPointerDownHandler : IEventSystemHandler { void OnPointerDown(PointerEventData eventData); }
    public interface IPointerUpHandler : IEventSystemHandler { void OnPointerUp(PointerEventData eventData); }
    public interface IPointerEnterHandler : IEventSystemHandler { void OnPointerEnter(PointerEventData eventData); }
    public interface IPointerExitHandler : IEventSystemHandler { void OnPointerExit(PointerEventData eventData); }

    public class EventSystem : Behaviour
    {
        public static EventSystem current;
        public EventSystem() { current = this; }
    }

    public class StandaloneInputModule : Behaviour { }

    public static class RectTransformUtility
    {
        public static bool ScreenPointToLocalPointInRectangle(
            RectTransform rect,
            Vector2 screenPoint,
            Camera cam,
            out Vector2 localPoint)
        {
            localPoint = screenPoint;
            return true;
        }
    }
}

namespace UnityEngine.UI
{
    using UnityEngine.Events;
    using UnityEngine.EventSystems;

    public class Canvas : Behaviour
    {
        public RenderMode renderMode;
        public int sortingOrder;
    }

    public class CanvasScaler : Behaviour
    {
        public enum ScaleMode { ConstantPixelSize, ScaleWithScreenSize, ConstantPhysicalSize }
        public enum ScreenMatchMode { MatchWidthOrHeight, Expand, Shrink }
        public ScaleMode uiScaleMode;
        public Vector2 referenceResolution;
        public ScreenMatchMode screenMatchMode;
        public float matchWidthOrHeight;
    }

    public class GraphicRaycaster : Behaviour { }

    public class RectMask2D : Behaviour { }

    public class Graphic : Behaviour
    {
        public Color color = Color.white;
        public bool raycastTarget = true;
        public RectTransform rectTransform => (RectTransform)transform;
    }

    public class Image : Graphic
    {
        public enum Type { Simple, Sliced, Tiled, Filled }
        public enum FillMethod { Horizontal, Vertical, Radial90, Radial180, Radial360 }
        public enum OriginHorizontal { Left, Right }
        public Sprite sprite;
        public Type type;
        public FillMethod fillMethod;
        public int fillOrigin;
        public float fillAmount = 1f;
    }

    public class Text : Graphic
    {
        public Font font;
        public string text = string.Empty;
        public int fontSize = 14;
        public TextAnchor alignment;
        public FontStyle fontStyle;
        public bool supportRichText = true;
        public float lineSpacing = 1f;
        public HorizontalWrapMode horizontalOverflow;
        public VerticalWrapMode verticalOverflow;
    }

    public struct ColorBlock
    {
        public Color normalColor;
        public Color highlightedColor;
        public Color pressedColor;
        public Color selectedColor;
        public Color disabledColor;
        public float colorMultiplier;
        public float fadeDuration;
    }

    public class Selectable : Behaviour
    {
        public enum Transition { None, ColorTint, SpriteSwap, Animation }
        public Transition transition;
        public ColorBlock colors;
        public Graphic targetGraphic;
        public bool interactable = true;
    }

    public class Button : Selectable
    {
        public class ButtonClickedEvent
        {
            private readonly List<UnityAction> _listeners = new List<UnityAction>();
            public void AddListener(UnityAction call) => _listeners.Add(call);
            public void RemoveAllListeners() => _listeners.Clear();
            public void Invoke() { for (int i = 0; i < _listeners.Count; i++) _listeners[i](); }
        }

        public ButtonClickedEvent onClick = new ButtonClickedEvent();
    }

    public class ScrollRect : Behaviour
    {
        public enum MovementType { Unrestricted, Elastic, Clamped }
        public RectTransform viewport;
        public RectTransform content;
        public bool horizontal = true;
        public bool vertical = true;
        public MovementType movementType;
        public float elasticity = 0.1f;
        public bool inertia = true;
        public float decelerationRate = 0.135f;
        public float scrollSensitivity = 1f;
    }

    public class GridLayoutGroup : Behaviour
    {
        public enum Constraint { Flexible, FixedColumnCount, FixedRowCount }
        public Vector2 cellSize;
        public Vector2 spacing;
        public RectOffset padding;
        public Constraint constraint;
        public int constraintCount;
        public TextAnchor childAlignment;
    }

    public class ContentSizeFitter : Behaviour
    {
        public enum FitMode { Unconstrained, MinSize, PreferredSize }
        public FitMode horizontalFit;
        public FitMode verticalFit;
    }

    public class RectOffset
    {
        public int left;
        public int right;
        public int top;
        public int bottom;
        public RectOffset() { }
        public RectOffset(int left, int right, int top, int bottom)
        {
            this.left = left;
            this.right = right;
            this.top = top;
            this.bottom = bottom;
        }
    }
}
