﻿using UnityEngine;

public class LoadingGUI : GUIPanel {
    public override void OnEnable() {
        holdOpen = true;
        base.OnEnable();
    }

    public override Rect GetRect(Rect safeRect, Rect screenRect) =>
        GUIUtils.CenterRect(safeRect.center.x, safeRect.center.y, 432, 216);

    public override void WindowGUI() {
        GUILayout.FlexibleSpace();
        GUILayout.Label(StringSet.LoadingWorld, GUIUtils.LABEL_HORIZ_CENTERED.Value);
        GUILayout.FlexibleSpace();
    }
}
