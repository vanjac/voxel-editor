using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LoadingGUI : GUIPanel
{

    public override void OnGUI()
    {
        base.OnGUI();

        panelRect = new Rect(scaledScreenWidth / 2 - targetHeight * 0.2f, targetHeight * 0.3f, targetHeight * 0.4f, targetHeight * 0.4f);

        GUI.Box(panelRect, "");

        GUI.skin.label.alignment = TextAnchor.MiddleCenter;
        GUI.Label(panelRect, "Loading map...");
        GUI.skin.label.alignment = TextAnchor.UpperLeft;
    }
}
