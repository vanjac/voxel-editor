﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverflowMenuGUI : GUIPanel
{
    private Texture worldIcon;
    private float buttonHeight;

    public override Rect GetRect(float width, float height)
    {
        buttonHeight = height * .12f;
        return new Rect(width - height * .4f, height * .13f, height * .4f, 0);
    }

    public override GUIStyle GetStyle()
    {
        return GUIStyle.none;
    }

    public override void OnEnable()
    {
        stealFocus = false;
        ActionBarGUI actionBar = GetComponent<ActionBarGUI>();
        if (actionBar != null)
        {
            worldIcon = actionBar.worldIcon;
        }
        base.OnEnable();
    }

    public override void WindowGUI()
    {
        if (MenuButton("World", worldIcon))
        {
            PropertiesGUI propsGUI = GetComponent<PropertiesGUI>();
            if (propsGUI != null)
            {
                propsGUI.worldSelected = true;
                propsGUI.normallyOpen = true;
            }
        }
    }

    private bool MenuButton(string name, Texture icon)
    {
        bool pressed = GUILayout.Button(name, GUILayout.Height(buttonHeight));
        Rect iconRect = GUILayoutUtility.GetLastRect();
        iconRect.xMin += iconRect.height * .1f;
        iconRect.yMin += iconRect.height * .1f;
        iconRect.height *= .8f;
        iconRect.width = iconRect.height;
        GUI.DrawTexture(iconRect, icon);
        if (pressed)
            Destroy(this);
        return pressed;
    }
}
