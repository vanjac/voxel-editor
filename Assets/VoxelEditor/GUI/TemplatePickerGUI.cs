﻿using UnityEngine;

public class TemplatePickerGUI : GUIPanel {
    private GUIContent[] options;
    public System.Action<int> handler;

    public static readonly System.Lazy<GUIStyle> buttonStyle = new System.Lazy<GUIStyle>(() => {
        var style = new GUIStyle(StyleSet.buttonSmall);
        style.imagePosition = ImagePosition.ImageAbove;
        return style;
    });

    public override Rect GetRect(Rect safeRect, Rect screenRect) =>
        GUIUtils.HorizCenterRect(safeRect.center.x, 180, 900, 480);

    void Start() {
        title = StringSet.CreateNewWorld;
        options = new GUIContent[] {
            new GUIContent(StringSet.IndoorWorld, IconSet.indoorLarge),
            new GUIContent(StringSet.FloatingWorld, IconSet.floatingLarge)
        };
    }

    public override void WindowGUI() {
        int selection = GUILayout.SelectionGrid(-1, options, 2, buttonStyle.Value, GUILayout.ExpandHeight(true));
        if (selection != -1) {
            handler(selection);
            Destroy(this);
        }
    }
}
