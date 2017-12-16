using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropertyGUIs
{
    static TouchScreenKeyboard numberKeyboard = null;

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

    public static object Float(object value)
    {
        float fValue = (float)value;

        string sValue = "";
        if (!float.IsNaN(fValue))
            sValue = fValue.ToString();

        if (TouchScreenKeyboard.isSupported)
        {
            if (GUILayout.Button(sValue, GUI.skin.textField))
                if (numberKeyboard == null)
                {
                    numberKeyboard =
                        TouchScreenKeyboard.Open(sValue, TouchScreenKeyboardType.NumbersAndPunctuation);
                    return float.NaN;
                }
            if (float.IsNaN(fValue)) // keyboard should be open
            {
                if (numberKeyboard == null)
                    return 0.0f;
                else if (numberKeyboard.status != TouchScreenKeyboard.Status.Visible)
                {
                    try
                    {
                        return float.Parse(numberKeyboard.text);
                    }
                    catch (FormatException)
                    {
                        return fValue;
                    }
                    finally
                    {
                        numberKeyboard = null;
                    }
                }
                else
                    return float.NaN;
            }
            else
                return fValue;
        }
        else // TouchScreenKeyboard not supported
        {
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

    public static object Tag(object value)
    {
        int tag = (byte)value;
        tag = GUILayout.SelectionGrid(tag,
            new string[] { "A", "B", "C" }, 3);
        return (byte)tag;
    }

    public static object BehaviorCondition(object value)
    {
        var gridStyle = new GUIStyle(GUI.skin.button);
        gridStyle.padding.left = 0;
        gridStyle.padding.right = 0;
        var condition = (EntityBehavior.Condition)value;
        condition = (EntityBehavior.Condition)GUILayout.SelectionGrid(
            (int)condition, new string[] { "On", "Off", "Both" }, 3, gridStyle);
        return condition;
    }
}
