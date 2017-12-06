using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Getters
{
    public delegate bool BoolGetter();
    public delegate int IntGetter();

    public static GetProperty Bool(BoolGetter boolGetter)
    {
        return () => boolGetter().ToString();
    }

    public static GetProperty Int(IntGetter intGetter)
    {
        return () => intGetter().ToString();
    }
}

public class Setters
{
    public delegate void BoolSetter(bool b);
    public delegate void IntSetter(int i);

    private static bool parseBool(string s)
    {
        try
        {
            return bool.Parse(s);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static int parseInt(string s)
    {
        try
        {
            return int.Parse(s);
        }
        catch (FormatException)
        {
            return 0;
        }
    }

    public static SetProperty Bool(BoolSetter boolSetter)
    {
        return s => boolSetter(parseBool(s));
    }

    public static SetProperty Int(IntSetter intSetter)
    {
        return s => intSetter(parseInt(s));
    }
}

public class PropertyGUIs
{
    public static string Empty(string value)
    {
        return value;
    }

    public static string Text(string value)
    {
        return GUILayout.TextField(value);
    }

    public static string Toggle(string value)
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        bool toggleValue = GUILayout.Toggle(bool.Parse(value), "");
        GUILayout.EndHorizontal();
        return toggleValue.ToString();
    }

    public static string Tag(string value)
    {
        int tag = int.Parse(value);
        GUILayout.BeginHorizontal();
        tag = GUILayout.SelectionGrid(tag,
            new string[] { "Grey", "Red", "Orange", "Yellow", "Green", "Blue", "Cyan", "Purple" },
            2, GUILayout.ExpandWidth(true));
        GUILayout.EndHorizontal();
        return tag.ToString();
    }
}
