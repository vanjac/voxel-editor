using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreditsGUI : GUIPanel
{
    public TextAsset creditsText;

    public override Rect GetRect(float width, float height)
    {
        return new Rect(width * .8f, 0, width * .2f, 0);
    }

    public override GUIStyle GetStyle()
    {
        return GUIStyle.none;
    }

    public override void OnEnable()
    {
        holdOpen = true;
        stealFocus = false;
        base.OnEnable();
    }

    public override void WindowGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if(GUILayout.Button("About", GUI.skin.GetStyle("button_large"),
                GUILayout.ExpandWidth(false)))
            LargeMessageGUI.ShowLargeMessageDialog(gameObject, creditsText.text);
        GUILayout.EndHorizontal();
    }
}