using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace MasterParking.UI
{
    /// <summary>
    /// Drag-to-steer wheel. Drag around the hub changes an absolute steering
    /// ratio; releasing lets the core simulation return the wheels to centre.
    /// </summary>
    public sealed class SteeringWheelControl : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        public event Action<float, bool> SteeringChanged;

        private const float MaxRotation = 140f;

        private RectTransform _touchRect;
        private RectTransform _wheelRect;
        private Image _touchImage;
        private Color _touchColor;
        private float _ratio;
        private float _lastAngle;
        private bool _dragging;
        private bool _interactable = true;

        public void Initialize(RectTransform touchRect, RectTransform wheelRect, Image touchImage)
        {
            _touchRect = touchRect;
            _wheelRect = wheelRect;
            _touchImage = touchImage;
            _touchColor = touchImage != null ? touchImage.color : Color.white;
            ApplyVisual();
        }

        public void SetInteractable(bool interactable)
        {
            _interactable = interactable;
            if (!interactable)
            {
                StopDragging();
            }

            if (_touchImage != null)
            {
                _touchImage.color = interactable
                    ? _touchColor
                    : new Color(_touchColor.r * 0.45f, _touchColor.g * 0.45f, _touchColor.b * 0.45f, _touchColor.a);
            }
        }

        private void Update()
        {
            if (!_dragging && Mathf.Abs(_ratio) > 0.001f)
            {
                _ratio = Mathf.MoveTowards(_ratio, 0f, 2.4f * Time.deltaTime);
                ApplyVisual();
            }
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!_interactable)
            {
                return;
            }

            _dragging = true;
            _lastAngle = PointerAngle(eventData);
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (!_dragging || !_interactable)
            {
                return;
            }

            float angle = PointerAngle(eventData);
            float delta = Mathf.DeltaAngle(_lastAngle, angle);
            _lastAngle = angle;
            _ratio = Mathf.Clamp(_ratio - delta / MaxRotation, -1f, 1f);
            ApplyVisual();
            SteeringChanged?.Invoke(_ratio, true);
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            StopDragging();
        }

        private void StopDragging()
        {
            if (!_dragging)
            {
                return;
            }

            _dragging = false;
            SteeringChanged?.Invoke(_ratio, false);
        }

        private void ApplyVisual()
        {
            if (_wheelRect != null)
            {
                _wheelRect.localRotation = Quaternion.Euler(0f, 0f, -_ratio * MaxRotation);
            }
        }

        private float PointerAngle(PointerEventData eventData)
        {
            if (_touchRect == null)
            {
                return 0f;
            }

            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    _touchRect,
                    eventData.position,
                    eventData.pressEventCamera,
                    out Vector2 local))
            {
                return _lastAngle;
            }

            return Mathf.Atan2(local.y, local.x) * Mathf.Rad2Deg;
        }

        private void OnDisable()
        {
            StopDragging();
        }
    }
}
