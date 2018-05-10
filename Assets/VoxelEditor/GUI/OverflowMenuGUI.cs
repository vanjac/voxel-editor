using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverflowMenuGUI : GUIPanel
{
    public VoxelArrayEditor voxelArray;
    public TouchListener touchListener;

    public override Rect GetRect(float width, float height)
    {
        return new Rect(width - height * .4f, height * .13f, height * .4f, 0);
    }

    public override GUIStyle GetStyle()
    {
        return GUIStyle.none;
    }

    public override void OnEnable()
    {
        stealFocus = false;
        base.OnEnable();
    }

    public override void WindowGUI()
    {
        if (MenuButton("World", GUIIconSet.instance.world))
        {
            PropertiesGUI propsGUI = GetComponent<PropertiesGUI>();
            if (propsGUI != null)
            {
                propsGUI.worldSelected = true;
                propsGUI.normallyOpen = true;
            }
        }
        if (MenuButton("Help", GUIIconSet.instance.help))
        {
            var help = gameObject.AddComponent<HelpGUI>();
            help.voxelArray = voxelArray;
            help.touchListener = touchListener;
        }
    }

    private bool MenuButton(string name, Texture icon)
    {
        bool pressed = GUILayout.Button(name, GUI.skin.GetStyle("button_large"));
        Rect iconRect = GUILayoutUtility.GetLastRect();
        iconRect.width = iconRect.height;
        GUI.Label(iconRect, icon, GUIUtils.LABEL_CENTERED.Value);
        if (pressed)
            Destroy(this);
        return pressed;
    }
}
