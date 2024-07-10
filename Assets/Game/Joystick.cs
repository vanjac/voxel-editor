using UnityEngine;
using UnityEngine.EventSystems;

// modified from Unity standard assets!

namespace UnityStandardAssets.CrossPlatformInput {
    public class Joystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler {
        private const float DOTS_PER_INCH = 72.0f;

        public float MovementRange = 30; // in dots

        private Vector3 m_StartPos;
        private Vector3 m_StartDrag;

        public int dragTouchId = -4;

        void Start() {
            m_StartPos = transform.position;
        }

        void UpdateVirtualAxes(Vector3 value) {
            var delta = value - m_StartPos;
            delta /= MovementRange * Screen.dpi / DOTS_PER_INCH;
            GameInput.virtJoystick = delta;
        }

        public void OnDrag(PointerEventData data) {
            Vector3 newPos = new Vector3(
                data.position.x - m_StartDrag.x,
                data.position.y - m_StartDrag.y,
                0);
            newPos = Vector3.ClampMagnitude(newPos, MovementRange * Screen.dpi / DOTS_PER_INCH);

            transform.position = m_StartPos + newPos;
            UpdateVirtualAxes(transform.position);
        }


        public void OnPointerUp(PointerEventData data) {
            transform.position = m_StartPos;
            UpdateVirtualAxes(m_StartPos);
            dragTouchId = -4;
        }


        public void OnPointerDown(PointerEventData data) {
            m_StartDrag = data.position;
            dragTouchId = data.pointerId;
        }
    }
}