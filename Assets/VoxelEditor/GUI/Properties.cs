using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropertyGUIs
{
    private static TouchScreenKeyboard numberKeyboard = null;
    private delegate void KeyboardHandler(string text);
    private static KeyboardHandler keyboardHandler;

    public static void Empty(Property property) { }

    public static void Text(Property property)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(property.name + " ", GUILayout.ExpandWidth(false));
        property.value = GUILayout.TextField((string)property.value);
        GUILayout.EndHorizontal();
    }

    public static void Toggle(Property property)
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(property.name + " ", GUILayout.ExpandWidth(false));
        GUILayout.FlexibleSpace();
        property.value = GUILayout.Toggle((bool)property.value, "");
        GUILayout.EndHorizontal();
    }

    public static void Float(Property property)
    {
        float fValue = (float)property.value;
        string sValue = fValue.ToString();

        GUILayout.BeginHorizontal();
        GUILayout.Label(property.name + " ", GUILayout.ExpandWidth(false));
        if (TouchScreenKeyboard.isSupported)
        {
            if (numberKeyboard != null && numberKeyboard.status != TouchScreenKeyboard.Status.Visible)
            {
                keyboardHandler(numberKeyboard.text);
                numberKeyboard = null;
                keyboardHandler = null;
            }
            if (GUILayout.Button(sValue, GUI.skin.textField) && numberKeyboard == null)
            {
                numberKeyboard = TouchScreenKeyboard.Open(sValue,
                    TouchScreenKeyboardType.NumbersAndPunctuation);
                keyboardHandler = text =>
                {
                    try
                    {
                        property.value = float.Parse(text);
                    }
                    catch (FormatException) { }
                };
            }
        }
        else // TouchScreenKeyboard not supported
        {
            sValue = GUILayout.TextField(sValue);
            try
            {
                property.value = float.Parse(sValue);
            }
            catch (FormatException) { }
        }
        GUILayout.EndHorizontal();
    }

    public static void Int(Property property)
    {
        Property wrapper = new Property(
            property.name,
            () => (float)(int)property.value,
            v => property.value = (int)(float)v,
            PropertyGUIs.Empty);
        Float(wrapper);
    }

    public static void Time(Property property)
    {
        Float(property);
    }

    public static void Tag(Property property)
    {
        int tag = (byte)property.value;
        GUILayout.Label(property.name);
        tag = GUILayout.SelectionGrid(tag, new string[]
        {
            Entity.TagToString(0),
            Entity.TagToString(1),
            Entity.TagToString(2)
        }, 3);
        property.value = (byte)tag;
    }

    public static void BehaviorCondition(Property property)
    {
        var gridStyle = new GUIStyle(GUI.skin.button);
        gridStyle.padding.left = 0;
        gridStyle.padding.right = 0;
        var condition = (EntityBehavior.Condition)property.value;
        GUILayout.Label(property.name);
        property.value = (EntityBehavior.Condition)GUILayout.SelectionGrid(
            (int)condition, new string[] { "On", "Off", "Both" }, 3, gridStyle);
    }

    public static void Filter(Property property)
    {
        var filter = (ActivatedSensor.Filter)property.value;
        GUILayout.BeginHorizontal();
        GUILayout.Label(property.name + " ", GUILayout.ExpandWidth(false));
        if (GUILayout.Button(filter.ToString(), GUI.skin.textField)) { }
        GUILayout.EndHorizontal();
    }
}
