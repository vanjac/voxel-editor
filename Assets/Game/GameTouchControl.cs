using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityStandardAssets.CrossPlatformInput;

public class GameTouchControl : MonoBehaviour
{
    CrossPlatformInputManager.VirtualAxis hAxis, vAxis;
    int lookTouchId;

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
        bool setAxes = false;
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);
            if (touch.phase == TouchPhase.Began && !EventSystem.current.IsPointerOverGameObject(touch.fingerId))
            {
                lookTouchId = touch.fingerId;
            }
            if (touch.fingerId == lookTouchId)
            {
                setAxes = true;
                hAxis.Update(touch.deltaPosition.x * 150f / Camera.current.pixelHeight);
                vAxis.Update(touch.deltaPosition.y * 150f / Camera.current.pixelHeight);
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
