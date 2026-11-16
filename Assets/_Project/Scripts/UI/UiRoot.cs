using MasterParking.Utilities;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MasterParking.UI
{
    /// <summary>
    /// One screen canvas. Owns the EventSystem once, a background layer and a
    /// safe-area content layer shared by all screens.
    /// </summary>
    public sealed class UiRoot : MonoBehaviour
    {
        public Canvas Canvas { get; private set; }
        public RectTransform SafeArea { get; private set; }
        public RectTransform Background { get; private set; }

        public static UiRoot Create(string name, int sortingOrder, bool withBackground = true)
        {
            EnsureEventSystem();

            var gameObject = new GameObject(
                name,
                typeof(RectTransform),
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster));

            var canvas = gameObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = sortingOrder;

            var scaler = gameObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            UiRoot root = gameObject.AddComponent<UiRoot>();
            root.Canvas = canvas;
            root.Build(withBackground);
            return root;
        }

        private void Build(bool withBackground)
        {
            Background = UiFactory.Rect("Background", transform);
            UiFactory.Stretch(Background);
            if (withBackground)
            {
                Image image = UiFactory.Image(
                    Background,
                    "Gradient",
                    Color.white,
                    TextureFactory.VerticalGradient(UiTheme.BackgroundTop, UiTheme.BackgroundBottom),
                    false);
                UiFactory.Stretch(image.rectTransform);
            }

            SafeArea = UiFactory.Rect("SafeArea", transform);
            UiFactory.Stretch(SafeArea);
            SafeArea.gameObject.AddComponent<SafeAreaFitter>();
        }

        private static void EnsureEventSystem()
        {
            if (EventSystem.current != null)
            {
                return;
            }

            var eventSystem = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            Object.DontDestroyOnLoad(eventSystem);
        }
    }
}
