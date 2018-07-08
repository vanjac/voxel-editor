using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropertyGUIs
{
    private static TouchScreenKeyboard numberKeyboard = null;
    private delegate void KeyboardHandler(string text);
    private static KeyboardHandler keyboardHandler;

    private static readonly Lazy<GUIStyle> alignedLabelStyle = new Lazy<GUIStyle>(() =>
    {
        var style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.MiddleLeft;
        style.padding.right = 0;
        return style;
    });
    private static readonly Lazy<GUIStyle> tagFieldStyle = new Lazy<GUIStyle>(() =>
    {
        var style = new GUIStyle(GUI.skin.textField);
        style.fontSize = GUI.skin.font.fontSize * 2;
        return style;
    });

    private static void AlignedLabel(Property property)
    {
        GUILayout.Label(property.name, alignedLabelStyle.Value, GUILayout.ExpandWidth(false));
    }

    public static void Empty(Property property) { }

    public static void Text(Property property)
    {
        GUILayout.BeginHorizontal();
        AlignedLabel(property);
        property.value = GUILayout.TextField((string)property.value);
        GUILayout.EndHorizontal();
    }

    public static void Toggle(Property property)
    {
        GUILayout.BeginHorizontal();
        AlignedLabel(property);
        GUILayout.FlexibleSpace();
        property.value = GUILayout.Toggle((bool)property.value, "");
        GUILayout.EndHorizontal();
    }

    public static void Float(Property property)
    {
        float fValue = (float)property.value;
        string sValue = fValue.ToString();

        GUILayout.BeginHorizontal();
        AlignedLabel(property);
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
        AlignedLabel(property);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(" " + tagString + " ", tagFieldStyle.Value, GUILayout.ExpandWidth(false)))
        {
            TagPickerGUI picker = GUIPanel.guiGameObject.AddComponent<TagPickerGUI>();
            picker.title = "Change " + property.name;
            picker.handler = (byte tag) =>
            {
                property.value = tag;
            };
        }
        GUILayout.EndHorizontal();
    }

    public static void BehaviorCondition(Property property)
    {
        var condition = (EntityBehavior.Condition)property.value;
        GUILayout.Label("When sensor is:");
        TutorialGUI.TutorialHighlight("behavior condition");
        property.value = (EntityBehavior.Condition)GUILayout.SelectionGrid(
            (int)condition, new string[] { "On", "Off", "Both" }, 3, GUI.skin.GetStyle("button_tab"));
        TutorialGUI.ClearHighlight();
    }

    public static void ActivatorBehaviorCondition(Property property)
    {
        GUILayout.Label("When sensor is On");
        property.value = EntityBehavior.Condition.ON;
    }

    public static void EntityReference(Property property)
    {
        _EntityReferenceCustom(property, false, "None");
    }

    public static void EntityReferenceWithNull(Property property)
    {
        _EntityReferenceCustom(property, true, "None");
    }

    public static void _EntityReferenceCustom(Property property, bool allowNull, string nullName)
    {
        var reference = (EntityReference)property.value;
        string valueString = nullName;

        Color baseColor = GUI.color;
        if (reference.entity != null)
        {
            EntityReferencePropertyManager.Next(reference.entity);
            GUI.color = baseColor * EntityReferencePropertyManager.GetColor();
            valueString = EntityReferencePropertyManager.GetName();
        }

        GUILayout.BeginHorizontal();
        AlignedLabel(property);
        if (GUILayout.Button(valueString, GUI.skin.textField))
        {
            EntityPickerGUI picker = GUIPanel.guiGameObject.AddComponent<EntityPickerGUI>();
            picker.voxelArray = VoxelArrayEditor.instance;
            picker.allowNone = false;
            picker.allowMultiple = false;
            picker.allowNull = allowNull;
            picker.nullName = nullName;
            picker.handler = (ICollection<Entity> entities) =>
            {
                foreach (Entity entity in entities)
                {
                    property.value = new EntityReference(entity);
                    return;
                }
                property.value = null;
            };
        }
        GUILayout.EndHorizontal();

        GUI.color = baseColor;
    }

    public static void Filter(Property property)
    {
        var filter = (ActivatedSensor.Filter)property.value;
        string filterString = filter.ToString();

        Color baseColor = GUI.color;
        ActivatedSensor.EntityFilter entityFilter = filter as ActivatedSensor.EntityFilter;
        if (entityFilter != null)
        {
            Entity e = entityFilter.entityRef.entity;
            if (e != null)
            {
                EntityReferencePropertyManager.Next(e);
                GUI.color = baseColor * EntityReferencePropertyManager.GetColor();
                filterString = EntityReferencePropertyManager.GetName();
            }
        }

        GUILayout.BeginHorizontal();
        AlignedLabel(property);
        if (GUILayout.Button(filterString, GUI.skin.textField))
        {
            FilterGUI filterGUI = GUIPanel.guiGameObject.AddComponent<FilterGUI>();
            filterGUI.title = property.name + " by...";
            filterGUI.voxelArray = VoxelArrayEditor.instance;
            filterGUI.handler = (ActivatedSensor.Filter newFilter) =>
            {
                property.value = newFilter;
            };
        }
        GUILayout.EndHorizontal();

        GUI.color = baseColor;
    }

    public static PropertyGUI Material(string materialDirectory, bool allowAlpha = false,
        MaterialSelectorGUI.ColorModeSet colorModeSet = MaterialSelectorGUI.ColorModeSet.DEFAULT)
    {
        return (Property property) =>
        {
            if (GUILayout.Button("Change " + property.name))
            {
                MaterialSelectorGUI materialSelector
                    = GUIPanel.guiGameObject.AddComponent<MaterialSelectorGUI>();
                materialSelector.title = "Change " + property.name;
                materialSelector.rootDirectory = materialDirectory;
                materialSelector.highlightMaterial = (Material)property.value;
                materialSelector.allowAlpha = allowAlpha;
                materialSelector.colorModeSet = colorModeSet;
                materialSelector.handler = (Material mat) =>
                {
                    property.setter(mat); // skip equality check, it could be the same material with a different color
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
            colorPicker.title = property.name;
            colorPicker.SetColor(valueColor);
            colorPicker.handler = (Color color) =>
            {
                property.value = color;
            };
        }
        GUI.color = baseColor;
    }

    public static void Target(Property property)
    {
        var target = (Target)property.value;
        string targetString = target.ToString();

        Color baseColor = GUI.color;
        if (target.entityRef.entity != null)
        {
            EntityReferencePropertyManager.Next(target.entityRef.entity);
            GUI.color = baseColor * EntityReferencePropertyManager.GetColor();
            targetString = EntityReferencePropertyManager.GetName();
        }

        GUILayout.BeginHorizontal();
        AlignedLabel(property);
        if (GUILayout.Button(targetString, GUI.skin.textField))
        {
            TargetGUI targetGUI = GUIPanel.guiGameObject.AddComponent<TargetGUI>();
            targetGUI.title = property.name;
            targetGUI.voxelArray = VoxelArrayEditor.instance;
            targetGUI.handler = (Target newTarget) =>
            {
                property.value = newTarget;
            };
        }
        GUILayout.EndHorizontal();

        GUI.color = baseColor;
    }
}
