using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityStandardAssets.CrossPlatformInput;

public class GameTouchControl : MonoBehaviour
{
    private Camera cam;
    private CrossPlatformInputManager.VirtualAxis hAxis, vAxis;
    private int lookTouchId;
    private TapComponent touchedTapComponent;

    void OnEnable ()
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

    void Update ()
    {
        if (cam == null)
        {
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
            if (touch.phase == TouchPhase.Began && !EventSystem.current.IsPointerOverGameObject(touch.fingerId))
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
                        touchedTapComponent.TapStart();
                    }
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
