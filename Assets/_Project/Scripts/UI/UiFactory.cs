using System;
using MasterParking.Audio;
using MasterParking.Utilities;
using UnityEngine;
using UnityEngine.UI;

namespace MasterParking.UI
{
    /// <summary>
    /// Small uGUI construction kit. Screens compose these helpers instead of
    /// hand-maintaining prefab wiring, and every button flows through the same
    /// click-sound hook.
    /// </summary>
    public static class UiFactory
    {
        public static RectTransform Rect(string name, Transform parent)
        {
            var gameObject = new GameObject(name, typeof(RectTransform));
            var rect = (RectTransform)gameObject.transform;
            rect.SetParent(parent, false);
            rect.localScale = Vector3.one;
            return rect;
        }

        public static RectTransform Stretch(RectTransform rect, float left = 0f, float bottom = 0f, float right = 0f, float top = 0f)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.offsetMin = new Vector2(left, bottom);
            rect.offsetMax = new Vector2(-right, -top);
            return rect;
        }

        public static void SetRect(
            RectTransform rect,
            Vector2 anchorMin,
            Vector2 anchorMax,
            Vector2 pivot,
            Vector2 anchoredPosition,
            Vector2 size)
        {
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = pivot;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;
        }

        public static Image Image(
            Transform parent,
            string name,
            Color color,
            Sprite sprite = null,
            bool raycast = false,
            UnityEngine.UI.Image.Type type = UnityEngine.UI.Image.Type.Simple)
        {
            RectTransform rect = Rect(name, parent);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = color;
            image.type = type;
            image.raycastTarget = raycast;
            return image;
        }

        public static Image Panel(Transform parent, string name, Color color, bool raycast = false)
        {
            return Image(
                parent,
                name,
                color,
                TextureFactory.RoundedRect(),
                raycast,
                UnityEngine.UI.Image.Type.Sliced);
        }

        public static Image Circle(Transform parent, string name, Color color, bool raycast = false)
        {
            return Image(parent, name, color, TextureFactory.Circle(), raycast);
        }

        public static Text Text(
            Transform parent,
            string name,
            string value,
            int size,
            Color color,
            TextAnchor alignment = TextAnchor.MiddleCenter,
            bool wrap = false,
            FontStyle style = FontStyle.Normal)
        {
            RectTransform rect = Rect(name, parent);
            var text = rect.gameObject.AddComponent<Text>();
            text.font = FontProvider.Get();
            text.text = value;
            text.fontSize = size;
            text.color = color;
            text.alignment = alignment;
            text.fontStyle = style;
            text.raycastTarget = false;
            text.supportRichText = true;
            text.horizontalOverflow = wrap ? HorizontalWrapMode.Wrap : HorizontalWrapMode.Overflow;
            text.verticalOverflow = VerticalWrapMode.Overflow;
            return text;
        }

        public static Button Button(
            Transform parent,
            string name,
            string label,
            int fontSize,
            Color background,
            Color foreground,
            Action onClick,
            AudioService audio)
        {
            Image image = Panel(parent, name, background, true);
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.ColorTint;

            ColorBlock colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(0.92f, 0.94f, 0.98f);
            colors.pressedColor = new Color(0.72f, 0.76f, 0.84f);
            colors.selectedColor = Color.white;
            colors.disabledColor = new Color(0.5f, 0.5f, 0.55f, 0.6f);
            colors.fadeDuration = 0.08f;
            button.colors = colors;

            Text text = Text(image.transform, "Label", label, fontSize, foreground, TextAnchor.MiddleCenter, false, FontStyle.Bold);
            Stretch(text.rectTransform, 14f, 6f, 14f, 6f);

            if (onClick != null)
            {
                button.onClick.AddListener(() =>
                {
                    audio?.PlayClick();
                    onClick();
                });
            }

            return button;
        }

        public static Image Star(Transform parent, string name, bool filled, float size = 42f)
        {
            Image image = Image(
                parent,
                name,
                filled ? UiTheme.Gold : UiTheme.StarEmpty,
                TextureFactory.Star(96),
                false);
            image.rectTransform.sizeDelta = new Vector2(size, size);
            return image;
        }

        public static Sprite SpriteFrom(Texture2D texture)
        {
            if (texture == null)
            {
                return null;
            }

            return Sprite.Create(
                texture,
                new Rect(0f, 0f, texture.width, texture.height),
                new Vector2(0.5f, 0.5f),
                100f);
        }

        public static Image SignSprite(Transform parent, string kind, Color color, float size)
        {
            Image image = Image(parent, "Sign_" + kind, color, SpriteFrom(TextureFactory.Sign(kind)), false);
            image.rectTransform.sizeDelta = new Vector2(size, size);
            return image;
        }
    }
}
