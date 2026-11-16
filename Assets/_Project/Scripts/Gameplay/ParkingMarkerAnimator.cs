using UnityEngine;

namespace MasterParking.Gameplay
{
    /// <summary>Small vertical bob so the parking bay is readable before entry.</summary>
    public sealed class ParkingMarkerAnimator : MonoBehaviour
    {
        private float _baseY;
        private float _phase;

        private void Awake()
        {
            _baseY = transform.localPosition.y;
            _phase = Random.value * Mathf.PI * 2f;
        }

        private void Update()
        {
            float y = _baseY + Mathf.Sin(Time.time * 2.4f + _phase) * 0.35f;
            Vector3 position = transform.localPosition;
            position.y = y;
            transform.localPosition = position;
            transform.Rotate(0f, 70f * Time.deltaTime, 0f, Space.Self);
        }
    }
}
