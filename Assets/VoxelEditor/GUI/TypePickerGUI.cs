using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TypePickerGUI : GUIPanel
{
    public delegate void TypeHandler(PropertiesObjectType type);

    public TypeHandler handler;
    public PropertiesObjectType[] items;

    public override Rect GetRect(float width, float height)
    {
        return new Rect(width * .25f, height * .1f, width * .5f, height * .8f);
    }

    public override void WindowGUI()
    {
        GUILayout.BeginScrollView(scroll);
        for (int i = 0; i < items.Length; i++)
        {
            PropertiesObjectType item = items[i];
            GUIPanel.BeginButtonHorizontal(item.fullName);
            GUILayout.Label(item.icon, GUILayout.ExpandWidth(false));
            GUILayout.BeginVertical();
            GUILayout.Label(item.fullName, GUI.skin.customStyles[0]);
            GUILayout.Label(item.description, GUI.skin.customStyles[1]);
            GUILayout.EndVertical();
            if (GUIPanel.EndButtonHorizontal(item.fullName))
            {
                handler(item);
                Destroy(this);
            }
        }
        GUILayout.EndScrollView();
    }
}