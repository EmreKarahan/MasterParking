using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace MasterParking.CameraSystem
{
    /// <summary>
    /// Transparent full-screen drag surface placed behind the HUD buttons.
    /// Dragging orbits the chase camera or pans the overview camera.
    /// </summary>
    public sealed class CameraDragArea : MonoBehaviour, IBeginDragHandler, IDragHandler
    {
        public event Action<Vector2> Dragged;

        public void OnBeginDrag(PointerEventData eventData)
        {
            Dragged?.Invoke(Vector2.zero);
        }

        public void OnDrag(PointerEventData eventData)
        {
            Dragged?.Invoke(eventData.delta);
        }
    }
}
