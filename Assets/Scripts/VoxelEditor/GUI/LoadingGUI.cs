using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class LoadingGUI : GUIPanel
{

    public override void OnGUI()
    {
        base.OnGUI();

        panelRect = new Rect(scaledScreenWidth / 2 - 100, targetHeight / 2 - 50, 200, 100);

        GUI.Box(panelRect, "");

        GUI.skin.label.alignment = TextAnchor.MiddleCenter;
        GUI.Label(panelRect, "Loading map...");
        GUI.skin.label.alignment = TextAnchor.UpperRight;
    }
}
