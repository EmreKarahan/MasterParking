using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MasterParking.UI
{
    /// <summary>Hold-to-drive pedal. Uses pointer down/up rather than Button clicks.</summary>
    public sealed class PedalButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IPointerEnterHandler
    {
        public event Action<bool> Pressed;

        private Image _image;
        private Color _normalColor;
        private bool _held;
        private bool _interactable = true;

        public void Initialize(Image image)
        {
            _image = image;
            _normalColor = image.color;
        }

        public void SetInteractable(bool interactable)
        {
            _interactable = interactable;
            if (!interactable && _held)
            {
                Release();
            }

            if (_image != null)
            {
                _image.color = interactable ? _normalColor : new Color(_normalColor.r * 0.5f, _normalColor.g * 0.5f, _normalColor.b * 0.5f, _normalColor.a);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_interactable)
            {
                return;
            }

            _held = true;
            if (_image != null)
            {
                _image.color = new Color(_normalColor.r * 0.7f, _normalColor.g * 0.7f, _normalColor.b * 0.7f, _normalColor.a);
            }

            Pressed?.Invoke(true);
        }

        public void OnPointerUp(PointerEventData eventData) => Release();
        public void OnPointerExit(PointerEventData eventData) => Release();
        public void OnPointerEnter(PointerEventData eventData)
        {
        }

        private void Release()
        {
            if (!_held)
            {
                return;
            }

            _held = false;
            if (_image != null)
            {
                _image.color = _interactable ? _normalColor : new Color(_normalColor.r * 0.5f, _normalColor.g * 0.5f, _normalColor.b * 0.5f, _normalColor.a);
            }

            Pressed?.Invoke(false);
        }

        private void OnDisable()
        {
            Release();
        }
    }
}
