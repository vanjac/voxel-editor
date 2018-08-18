using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverflowMenuGUI : GUIPanel
{
    public struct MenuItem
    {
        public string text;
        public Texture icon;
        public System.Action action;
        public bool stayOpen;

        public MenuItem(string text, Texture icon, System.Action action, bool stayOpen = false)
        {
            this.text = text;
            this.icon = icon;
            this.action = action;
            this.stayOpen = stayOpen;
        }
    }

    public MenuItem[] items;
    public int depth = 0;

    public override Rect GetRect(float width, float height)
    {
        return new Rect(width - height * .4f * (depth + 1), height * .13f, height * .4f, 0);
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
        foreach (MenuItem item in items)
        {
            if (MenuButton(item.text, item.icon))
            {
                item.action();
                if (!item.stayOpen)
                {
                    // destroy self and all parent menus
                    foreach(OverflowMenuGUI parentMenu in gameObject.GetComponents<OverflowMenuGUI>())
                        Destroy(parentMenu);
                }
            }
        }
    }

    private bool MenuButton(string name, Texture icon)
    {
        bool pressed = GUILayout.Button(name, GUI.skin.GetStyle("button_large"));
        Rect iconRect = GUILayoutUtility.GetLastRect();
        iconRect.width = iconRect.height;
        GUI.Label(iconRect, icon, GUIUtils.LABEL_CENTERED.Value);
        return pressed;
    }
}
