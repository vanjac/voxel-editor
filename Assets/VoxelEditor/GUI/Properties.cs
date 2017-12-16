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
            new string[] { "A", "B", "C" }, 3);
        GUILayout.EndHorizontal();
        return (byte)tag;
    }

    public static object Float(object value)
    {
        float fValue = (float)value;
        string sValue = "";
        if(!float.IsNaN(fValue))
            sValue = fValue.ToString();
        sValue = GUILayout.TextField(sValue);
        if (sValue.Length == 0)
            fValue = float.NaN;
        else
        {
            try
            {
                fValue = float.Parse(sValue);
            }
            catch (FormatException) { }
        }
        return fValue;
    }
}
