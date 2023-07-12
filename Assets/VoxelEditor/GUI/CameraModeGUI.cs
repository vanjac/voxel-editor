using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraModeGUI : GUIPanel
{
    public TouchListener touchListener;

    public override void OnEnable()
    {
        holdOpen = true;
        stealFocus = false;
        base.OnEnable();
    }

    public override GUIStyle GetStyle()
    {
        return GUIStyle.none;
    }

    public override Rect GetRect(Rect safeRect, Rect screenRect)
    {
        var style = GUIStyleSet.instance.buttonLarge;
        float width = style.margin.left * 2 + style.padding.left * 2 + GUIIconSet.instance.orbit.width;
        float height = style.fixedHeight;
        float yMax = (GUIPanel.bottomPanel != null) ? GUIPanel.bottomPanel.panelRect.yMin
            : safeRect.yMax;
        return new Rect(safeRect.xMax - width, yMax - height, width, height);
    }

    public override void WindowGUI()
    {
        var baseColor = GUI.backgroundColor;
        GUI.backgroundColor *= new Color(1, 1, 1, 0.5f);
        var icon = (touchListener.cameraMode == TouchListener.CameraMode.PAN) ?
            GUIIconSet.instance.pan : GUIIconSet.instance.orbit;
        if (GUILayout.Button(icon, GUIStyleSet.instance.buttonLarge))
        {
            touchListener.cameraMode = (touchListener.cameraMode == TouchListener.CameraMode.PAN) ?
                TouchListener.CameraMode.ORBIT : TouchListener.CameraMode.PAN;
        }
        GUI.backgroundColor = baseColor;
    }
}
