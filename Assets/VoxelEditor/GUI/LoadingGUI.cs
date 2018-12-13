using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LoadingGUI : GUIPanel
{
    public override void OnEnable()
    {
        holdOpen = true;
        base.OnEnable();
    }

    public override Rect GetRect(Rect maxRect)
    {
        return GUIUtils.CenterRect(maxRect.center.x, maxRect.center.y, 432, 216);
    }

    public override void WindowGUI()
    {
        GUILayout.FlexibleSpace();
        GUILayout.Label("Loading world...", GUIUtils.LABEL_HORIZ_CENTERED.Value);
        GUILayout.FlexibleSpace();
    }
}
