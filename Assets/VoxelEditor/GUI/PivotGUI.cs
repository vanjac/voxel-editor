using UnityEngine;

public class PivotGUI : GUIPanel
{
    public Pivot value;
    public System.Action<Pivot> handler;

    public override Rect GetRect(Rect safeRect, Rect screenRect)
    {
        return new Rect(GUIPanel.leftPanel.panelRect.xMax,
            GUIPanel.topPanel.panelRect.yMax, 1100, 0);
    }

    public override void OnEnable()
    {
        showCloseButton = true;
        base.OnEnable();
    }

    public override void WindowGUI()
    {
        Pivot oldValue = value;

        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical();
        Color baseColor = GUI.color;
        GUI.color = baseColor * new Color(0.6f, 0.6f, 1);
        value.z = (Pivot.Pos)GUILayout.SelectionGrid((int)value.z,
            new string[] { "South", "Center", "North" }, 3);
        GUI.color = baseColor * new Color(1, 0.6f, 0.6f);
        value.x = (Pivot.Pos)GUILayout.SelectionGrid((int)value.x,
            new string[] { "West", "Center", "East" }, 3);
        GUI.color = baseColor * new Color(0.6f, 1, 0.6f);
        value.y = (Pivot.Pos)GUILayout.SelectionGrid((int)value.y,
            new string[] { "Bottom", "Center", "Top" }, 3);
        GUI.color = baseColor;
        GUILayout.Space(16);  // fix weird layout issue
        GUILayout.EndVertical();

        GUILayout.BeginVertical();
        GUILayout.FlexibleSpace();
        GUILayout.Space(8);  // additional padding
        GUILayout.Box("", GUIStyle.none, GUILayout.Width(200), GUILayout.Height(200));
        TargetGUI.DrawCompass(this, GUILayoutUtility.GetLastRect());
        GUILayout.Space(8);
        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();

        if (!oldValue.Equals(value))
            handler(value);
    }
}
