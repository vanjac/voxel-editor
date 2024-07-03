using UnityEngine;

public class CameraModeGUI : GUIPanel {
    public TouchListener touchListener;

    public override void OnEnable() {
        holdOpen = true;
        stealFocus = false;
        base.OnEnable();
    }

    public override GUIStyle GetStyle() => GUIStyle.none;

    public override Rect GetRect(Rect safeRect, Rect screenRect) {
        var style = StyleSet.buttonLarge;
        float width = style.margin.left * 2 + style.padding.left * 2 + IconSet.orbit.width;
        float height = style.fixedHeight;
        float yMax = (GUIPanel.bottomPanel != null) ? GUIPanel.bottomPanel.panelRect.yMin
            : safeRect.yMax;
        return new Rect(safeRect.xMax - width, yMax - height, width, height);
    }

    public override void WindowGUI() {
        var baseColor = GUI.backgroundColor;
        GUI.backgroundColor *= new Color(1, 1, 1, 0.5f);
        var icon = (touchListener.cameraMode == TouchListener.CameraMode.PAN) ?
            IconSet.pan : IconSet.orbit;
        TutorialGUI.TutorialHighlight("pan");
        if (GUILayout.Button(icon, StyleSet.buttonLarge)) {
            touchListener.cameraMode = (touchListener.cameraMode == TouchListener.CameraMode.PAN) ?
                TouchListener.CameraMode.ORBIT : TouchListener.CameraMode.PAN;
        }
        TutorialGUI.ClearHighlight();
        GUI.backgroundColor = baseColor;
    }
}
