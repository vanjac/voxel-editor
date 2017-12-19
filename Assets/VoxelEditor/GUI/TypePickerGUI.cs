using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TypePickerGUI : GUIPanel
{
    public delegate void TypeHandler(System.Type type);

    public TypeHandler handler;
    public GameScripts.NamedType[] items;

    public override Rect GetRect(float width, float height)
    {
        return new Rect(width * .25f, height * .25f, width * .5f, height * .5f);
    }

    public override void WindowGUI()
    {
        for (int i = 0; i < items.Length; i++ )
            if (GUILayout.Button(items[i].name))
            {
                handler(items[i].type);
                Destroy(this);
            }
    }
}