using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityStandardAssets.CrossPlatformInput;

public class GameTouchControl : MonoBehaviour
{
    private const float CARRY_DISTANCE = 3;
    private Camera cam;
    private CrossPlatformInputManager.VirtualAxis hAxis, vAxis;
    private int lookTouchId;
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
            // TODO: don't use Camera.current!
            cam = Camera.current; // sometimes null for a few cycles
            if (cam != null && cam.tag == "DeathCamera")
                cam = null;
        }
        if (cam == null)
            return; // sometimes null for a few cycles
        bool setAxes = false;
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);
            if (touch.phase == TouchPhase.Began
                && !EventSystem.current.IsPointerOverGameObject(touch.fingerId)
                && GUIPanel.PanelContainingPoint(touch.position) == null)
            {
                if (touchedTapComponent != null)
                    touchedTapComponent.TapEnd();
                lookTouchId = touch.fingerId;

                RaycastHit hit;
                if (Physics.Raycast(cam.ScreenPointToRay(touch.position), out hit))
                {
                    TapComponent hitTapComponent = hit.transform.GetComponent<TapComponent>();
                    if (hitTapComponent != null)
                    {
                        touchedTapComponent = hitTapComponent;
                        touchedTapComponent.TapStart(PlayerComponent.instance, hit.distance);
                    }
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
                if (touch.phase == TouchPhase.Ended && touchedTapComponent != null)
                {
                    touchedTapComponent.TapEnd();
                    touchedTapComponent = null;
                }
            }
        }
        if (!setAxes)
        {
            lookTouchId = -1;
            hAxis.Update(0);
            vAxis.Update(0);
        }
    }
}
