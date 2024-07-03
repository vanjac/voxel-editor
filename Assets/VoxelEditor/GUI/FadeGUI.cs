using UnityEngine;

public class FadeGUI : GUIPanel {
    public Texture background;
    public float backgroundWidth, backgroundHeight;

    public override Rect GetRect(Rect safeRect, Rect screenRect) => screenRect;

    public override void OnEnable() {
        holdOpen = true;
        stealFocus = false;

        base.OnEnable();
    }

    public override GUIStyle GetStyle() => GUIStyle.none;

    public override void WindowGUI() {
        Color baseColor = GUI.backgroundColor;
        GUI.backgroundColor = Color.black;
        GUI.Box(panelRect, "");
        GUI.backgroundColor = baseColor;
        GUILayout.BeginVertical(GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
        GUILayout.FlexibleSpace();
        if (background != null) {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Box("", GUIStyle.none,
                GUILayout.Width(backgroundWidth),
                GUILayout.Height(backgroundHeight));
            GUI.DrawTexture(GUILayoutUtility.GetLastRect(), background);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
        }
        GUILayout.EndVertical();
    }
}