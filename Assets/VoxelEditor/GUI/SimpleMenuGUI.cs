using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMenuGUI : GUIPanel
{
    public delegate void MenuHandler(int itemI, string itemName);

    public MenuHandler handler;
    public string[] items;

    public override void OnEnable()
    {
        depth = -1;
        base.OnEnable();
    }

    public override void OnGUI()
    {
        base.OnGUI();

        panelRect = new Rect(scaledScreenWidth * .25f, targetHeight * .25f,
            scaledScreenWidth * .5f, targetHeight * .5f);
        GUILayout.BeginArea(panelRect, GUI.skin.box);

        for (int i = 0; i < items.Length; i++ )
            if (GUILayout.Button(items[i]))
            {
                handler(i, items[i]);
                Destroy(this);
            }

        GUILayout.EndArea();
    }
}