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
        return new Rect(width * .25f, height * .25f, width * .5f, height * .5f);
    }

    public override void WindowGUI()
    {
        GUILayout.BeginScrollView(scroll);
        for (int i = 0; i < items.Length; i++)
        {
            PropertiesObjectType item = items[i];
            GUILayout.BeginHorizontal(GUI.skin.box);
            GUILayout.Label(item.icon);
            GUILayout.BeginVertical();
            GUILayout.Label(item.fullName);
            GUILayout.Label(item.description);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            Rect buttonRect = GUILayoutUtility.GetLastRect();
            if (GUI.Button(buttonRect, "", GUIStyle.none))
            {
                handler(item);
                Destroy(this);
            }
        }
        GUILayout.EndScrollView();
    }
}