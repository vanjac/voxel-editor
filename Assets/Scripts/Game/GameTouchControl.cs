using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityStandardAssets.CrossPlatformInput;

public class GameTouchControl : MonoBehaviour {

    CrossPlatformInputManager.VirtualAxis hAxis, vAxis;
    bool uiInteraction = false;

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
	
	// Update is called once per frame
	void Update () {
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                uiInteraction = true;
            if(!uiInteraction)
            {
                hAxis.Update(touch.deltaPosition.x / 10);
                vAxis.Update(touch.deltaPosition.y / 10);
            }
            else
            {
                hAxis.Update(0);
                vAxis.Update(0);
            }
        }
        else
        {
            uiInteraction = false;
            hAxis.Update(0);
            vAxis.Update(0);
        }
	}
}
