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
        string tagString = Entity.TagToString((byte)property.value);
        GUILayout.BeginHorizontal();
        GUILayout.Label(property.name + " ", GUILayout.ExpandWidth(false));
        if (GUILayout.Button(tagString, GUI.skin.textField))
        {
            TagPickerGUI picker = GUIPanel.guiGameObject.AddComponent<TagPickerGUI>();
            picker.handler = (byte tag) =>
            {
                property.value = tag;
            };
        }
        GUILayout.EndHorizontal();
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

    public static PropertyGUI Material(string materialDirectory)
    {
        return (Property property) =>
        {
            if (GUILayout.Button("Set " + property.name))
            {
                MaterialSelectorGUI materialSelector
                    = GUIPanel.guiGameObject.AddComponent<MaterialSelectorGUI>();
                materialSelector.materialDirectory = materialDirectory;
                materialSelector.handler = (Material mat) =>
                {
                    property.value = mat;
                };
            }
        };
    }

    public static PropertyGUI Slider(float minValue, float maxValue)
    {
        return (Property property) =>
        {
            GUILayout.Label(property.name + ":");
            property.value = GUILayout.HorizontalSlider(
                (float)property.value, minValue, maxValue);
        };
    }

    public static void Color(Property property)
    {
        Color baseColor = GUI.color;
        Color valueColor = (Color)property.value;
        GUI.color = baseColor * valueColor;
        if (GUILayout.Button(property.name))
        {
            ColorPickerGUI colorPicker = GUIPanel.guiGameObject.AddComponent<ColorPickerGUI>();
            colorPicker.color = valueColor;
            colorPicker.handler = (Color color) =>
            {
                property.value = color;
            };
        }
        GUI.color = baseColor;
    }
}
