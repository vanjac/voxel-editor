using UnityEngine;
using UnityEngine.EventSystems;
using UnityStandardAssets.CrossPlatformInput;

public class GameTouchControl : MonoBehaviour
{
    private const float DRAG_THRESHOLD = 64;
    private const float CARRY_DISTANCE = 3;
    private Camera cam;
    private CrossPlatformInputManager.VirtualAxis hAxis, vAxis;
    private int lookTouchId;
    private Vector2 lookTouchStart;
    private TapComponent touchedTapComponent;
    private CarryableComponent carriedComponent;
    public Joystick joystick;

    void OnEnable()
    {
        hAxis = new CrossPlatformInputManager.VirtualAxis("Mouse X");
        CrossPlatformInputManager.RegisterVirtualAxis(hAxis);
        vAxis = new CrossPlatformInputManager.VirtualAxis("Mouse Y");
        CrossPlatformInputManager.RegisterVirtualAxis(vAxis);
    }

    void OnDisable()
    {
        hAxis.Remove();
        vAxis.Remove();
    }

    void Update()
    {
        if (cam == null)
        {
            cam = Camera.main; // sometimes null for a few cycles
            if (cam != null && cam.tag == "DeathCamera")
                cam = null;
        }
        if (cam == null)
            return; // sometimes null for a few cycles
        RaycastHit hit;
        bool setAxes = false;
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);
            if (touch.phase == TouchPhase.Began
                && !EventSystem.current.IsPointerOverGameObject(touch.fingerId)
                && GUIPanel.PanelContainingPoint(touch.position) == null)
            {
                if (touchedTapComponent != null)
                {
                    touchedTapComponent.TapEnd();
                    touchedTapComponent = null;
                }
                lookTouchId = touch.fingerId;
                lookTouchStart = touch.position;

                if (TapRaycast(touch.position, out hit))
                {
                    TapComponent hitTapComponent = hit.transform.GetComponent<TapComponent>();
                    if (hitTapComponent != null && hit.distance <= hitTapComponent.Distance)
                    {
                        touchedTapComponent = hitTapComponent;
                        touchedTapComponent.TapStart(PlayerComponent.instance);
                    }
                }
            }
            // don't move joystick and camera with same touch
            if (lookTouchId == joystick.dragTouchId)
            {
                lookTouchId = -1;
                if (touchedTapComponent != null)
                {
                    touchedTapComponent.TapEnd();
                    touchedTapComponent = null;
                }
            }
            if (touch.fingerId == lookTouchId)
            {
                setAxes = true;
                hAxis.Update(touch.deltaPosition.x * 150f / cam.pixelHeight);
                vAxis.Update(touch.deltaPosition.y * 150f / cam.pixelHeight);
                if ((touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary)
                    && touchedTapComponent != null)
                {
                    // check for early cancel (tap left component)
                    if (TapRaycast(touch.position, out hit))
                    {
                        TapComponent hitTapComponent = hit.transform.GetComponent<TapComponent>();
                        if (hitTapComponent != touchedTapComponent || hit.distance > touchedTapComponent.Distance)
                        {
                            touchedTapComponent.TapEnd();
                            touchedTapComponent = null;
                        }
                    }
                    else
                    {
                        touchedTapComponent.TapEnd();
                        touchedTapComponent = null;
                    }
                }
                if (touch.phase == TouchPhase.Ended && touchedTapComponent != null)
                {
                    touchedTapComponent.TapEnd();
                    touchedTapComponent = null;
                }
                if (touch.phase == TouchPhase.Ended
                    && (touch.position - lookTouchStart).magnitude / GUIPanel.scaleFactor < DRAG_THRESHOLD)
                {
                    if (TapRaycast(touch.position, out hit))
                    {
                        CarryableComponent hitCarryable = hit.transform.GetComponent<CarryableComponent>();
                        if (hitCarryable != null && hitCarryable.enabled)
                        {
                            if (hitCarryable.IsCarried())
                            {
                                hitCarryable.Throw(PlayerComponent.instance);
                            }
                            else if (hit.distance <= CARRY_DISTANCE)
                            {
                                if (carriedComponent != null && carriedComponent.IsCarried())
                                    carriedComponent.Drop();
                                hitCarryable.Carry(PlayerComponent.instance);
                                carriedComponent = hitCarryable;
                            }
                        }
                    }
                }
            }  // end if touch.fingerId == lookTouchId
        }
        if (!setAxes)
        {
            lookTouchId = -1;
            hAxis.Update(0);
            vAxis.Update(0);
        }
    }

    private bool TapRaycast(Vector2 touchPosition, out RaycastHit hit) =>
        Physics.Raycast(cam.ScreenPointToRay(touchPosition), out hit,
            Mathf.Infinity, Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore);
}
