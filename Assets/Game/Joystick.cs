using UnityEngine;
using UnityEngine.EventSystems;

// modified from Unity standard assets!

namespace UnityStandardAssets.CrossPlatformInput
{
    public class Joystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        private const float DOTS_PER_INCH = 72.0f;

        public float MovementRange = 30; // in dots
        public string horizontalAxisName = "Horizontal"; // The name given to the horizontal axis for the cross platform input
        public string verticalAxisName = "Vertical"; // The name given to the vertical axis for the cross platform input

        private Vector3 m_StartPos;
        private Vector3 m_StartDrag;
        private CrossPlatformInputManager.VirtualAxis m_HorizontalVirtualAxis; // Reference to the joystick in the cross platform input
        private CrossPlatformInputManager.VirtualAxis m_VerticalVirtualAxis; // Reference to the joystick in the cross platform input

        public int dragTouchId = -4;

        void OnEnable()
        {
            CreateVirtualAxes();
        }

        void Start()
        {
            m_StartPos = transform.position;
        }

        void UpdateVirtualAxes(Vector3 value)
        {
            var delta = m_StartPos - value;
            delta.y = -delta.y;
            delta /= MovementRange * Screen.dpi / DOTS_PER_INCH;
            m_HorizontalVirtualAxis.Update(-delta.x);
            m_VerticalVirtualAxis.Update(delta.y);
        }

        void CreateVirtualAxes()
        {
            // create new axes based on axes to use
            m_HorizontalVirtualAxis = new CrossPlatformInputManager.VirtualAxis(horizontalAxisName);
            CrossPlatformInputManager.RegisterVirtualAxis(m_HorizontalVirtualAxis);
            m_VerticalVirtualAxis = new CrossPlatformInputManager.VirtualAxis(verticalAxisName);
            CrossPlatformInputManager.RegisterVirtualAxis(m_VerticalVirtualAxis);
        }


        public void OnDrag(PointerEventData data)
        {
            Vector3 newPos = new Vector3(
                data.position.x - m_StartDrag.x,
                data.position.y - m_StartDrag.y,
                0);
            newPos = Vector3.ClampMagnitude(newPos, MovementRange * Screen.dpi / DOTS_PER_INCH);

            transform.position = m_StartPos + newPos;
            UpdateVirtualAxes(transform.position);
        }


        public void OnPointerUp(PointerEventData data)
        {
            transform.position = m_StartPos;
            UpdateVirtualAxes(m_StartPos);
            dragTouchId = -4;
        }


        public void OnPointerDown(PointerEventData data)
        {
            m_StartDrag = data.position;
            dragTouchId = data.pointerId;
        }

        void OnDisable()
        {
            // remove the joysticks from the cross platform input
            m_HorizontalVirtualAxis.Remove();
            m_VerticalVirtualAxis.Remove();
        }
    }
}