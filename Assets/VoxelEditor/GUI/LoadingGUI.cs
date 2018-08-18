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

    public override Rect GetRect(float width, float height)
    {
        return new Rect(width / 2 - height * 0.2f, height * 0.4f, height * 0.4f, height * 0.2f);
    }

    public override void WindowGUI()
    {
        GUILayout.FlexibleSpace();
        GUILayout.Label("Loading world...", GUIUtils.LABEL_HORIZ_CENTERED.Value);
        GUILayout.FlexibleSpace();
    }
}
