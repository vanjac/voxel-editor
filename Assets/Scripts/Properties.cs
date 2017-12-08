using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropertyGUIs
{
    public static object Empty(object value)
    {
        return value;
    }

    public static object Text(object value)
    {
        return GUILayout.TextField((string)value);
    }

    public static object Toggle(object value)
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        bool toggleValue = GUILayout.Toggle((bool)value, "");
        GUILayout.EndHorizontal();
        return toggleValue;
    }

    public static object Tag(object value)
    {
        int tag = (byte)value;
        GUILayout.BeginHorizontal();
        tag = GUILayout.SelectionGrid(tag,
            new string[] { "Grey", "Red", "Orange", "Yellow", "Green", "Blue", "Cyan", "Purple" },
            2, GUILayout.ExpandWidth(true));
        GUILayout.EndHorizontal();
        return (byte)tag;
    }
}
