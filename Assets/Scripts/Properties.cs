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

    public static SetProperty Bool(BoolSetter boolSetter)
    {
        return s => boolSetter(bool.Parse(s));
    }

    public static SetProperty Int(IntSetter intSetter)
    {
        return s => intSetter(int.Parse(s));
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
        return GUILayout.Toggle(bool.Parse(value), "").ToString();
    }
}
