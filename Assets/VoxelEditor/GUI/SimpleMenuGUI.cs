using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleMenuGUI : GUIPanel
{
    public string[] itemNames;
    public int highlightedIndex = -1;
    public System.Action<int> handler;

    public override Rect GetRect(Rect safeRect, Rect screenRect)
    {
        return new Rect(GUIPanel.leftPanel.panelRect.xMax,
            GUIPanel.topPanel.panelRect.yMax, 576, 800);
    }

    public override void WindowGUI()
    {
        scroll = GUILayout.BeginScrollView(scroll);
        int selected = GUILayout.SelectionGrid(highlightedIndex, itemNames, 1,
                                               GUIStyleSet.instance.buttonLarge);
        GUILayout.EndScrollView();
        if (selected != highlightedIndex)
        {
            handler(selected);
            Destroy(this);
        }
    }
}
