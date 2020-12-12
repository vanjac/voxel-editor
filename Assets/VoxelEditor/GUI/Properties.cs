using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PropertyGUIs
{
    private static TouchScreenKeyboard numberKeyboard = null;
    private delegate void KeyboardHandler(string text);
    private static KeyboardHandler keyboardHandler;

    private static readonly System.Lazy<GUIStyle> alignedLabelStyle = new System.Lazy<GUIStyle>(() =>
    {
        var style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.MiddleLeft;
        style.padding.right = 0;
        return style;
    });
    private static readonly System.Lazy<GUIStyle> headerLabelStyle = new System.Lazy<GUIStyle>(() =>
    {
        var style = new GUIStyle(GUI.skin.label);
        style.padding.top = 0;
        style.padding.bottom = 0;
        return style;
    });
    private static readonly System.Lazy<GUIStyle> inlineLabelStyle = new System.Lazy<GUIStyle>(() =>
    {
        var style = new GUIStyle(alignedLabelStyle.Value);
        style.padding.left = 0;
        return style;
    });
    private static readonly System.Lazy<GUIStyle> tagFieldStyle = new System.Lazy<GUIStyle>(() =>
    {
        var style = new GUIStyle(GUI.skin.textField);
        style.fontSize = GUI.skin.font.fontSize * 2;
        return style;
    });

    public static void AlignedLabel(Property property)
    {
        if (property.name != "")
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

    public static void DoubleToggle(Property property)
    {
        string[] names = property.name.Split('|');
        var values = ((bool, bool))property.value;
        var buttonStyle = GUIStyleSet.instance.buttonTab;
        GUILayout.BeginHorizontal();
        values.Item1 ^= GUIUtils.HighlightedButton(names[0], buttonStyle, values.Item1);
        values.Item2 ^= GUIUtils.HighlightedButton(names[1], buttonStyle, values.Item2);
        property.value = values;
        GUILayout.EndHorizontal();
    }

    public static void Enum(Property property)
    {
        System.Enum e = (System.Enum)property.value;
        var buttonStyle = GUIStyleSet.instance.buttonTab;
        GUILayout.BeginHorizontal();
        foreach (var enumValue in System.Enum.GetValues(e.GetType()))
        {
            string name = enumValue.ToString();
            // sentence case
            if (name[0] == '_')
                name = name.Substring(1).ToLower();
            else
                name = Char.ToUpper(name[0]) + name.Substring(1).ToLower();
            if (enumValue.Equals(e))
                GUIUtils.HighlightedButton(name, buttonStyle);
            else if (GUILayout.Button(name, buttonStyle))
                property.value = enumValue;
        }
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
            property.id,
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

    public static void FloatPair(Property property, string separator)
    {
        GUILayout.Label(property.name + ":", headerLabelStyle.Value);
        GUILayout.BeginHorizontal();
        var range = ((float, float))property.value;
        Property wrapper1 = new Property(
            property.id + "1",
            "",
            () => range.Item1,
            v => property.value = ((float)v, range.Item2),
            PropertyGUIs.Empty);
        Float(wrapper1);
        GUILayout.Label(separator, inlineLabelStyle.Value, GUILayout.ExpandWidth(false));
        Property wrapper2 = new Property(
            property.id + "2",
            "",
            () => range.Item2,
            v => property.value = (range.Item1, (float)v),
            PropertyGUIs.Empty);
        Float(wrapper2);
        GUILayout.EndHorizontal();
    }

    public static void FloatRange(Property property)
    {
        FloatPair(property, "to");
    }

    public static void FloatDimensions(Property property)
    {
        FloatPair(property, "x");
    }

    public static void Tag(Property property)
    {
        string tagString = Entity.TagToString((byte)property.value);
        GUILayout.BeginHorizontal();
        AlignedLabel(property);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(" " + tagString + " ", tagFieldStyle.Value, GUILayout.ExpandWidth(false)))
        {
            TagPickerGUI picker = GUIManager.guiGameObject.AddComponent<TagPickerGUI>();
            picker.title = "Change " + property.name;
            picker.multiSelection = (byte)(1 << (byte)property.value);
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
            (int)condition, new string[] { "On", "Off", "Both" }, 3, GUIStyleSet.instance.buttonTab);
        TutorialGUI.ClearHighlight();
    }

    public static void ActivatorBehaviorCondition(Property property)
    {
        GUILayout.Label("When sensor is On");
        property.value = EntityBehavior.Condition.ON;
    }

    public static void BehaviorTarget(Property property)
    {
        var value = (EntityBehavior.BehaviorTargetProperty)(property.value);
        Entity behaviorTarget = value.targetEntity.entity;
        string text;
        if (value.targetEntityIsActivator)
        {
            text = "Activators";
        }
        else if (behaviorTarget != null)
        {
            // only temporarily, so the name won't be "Target":
            EntityReferencePropertyManager.SetBehaviorTarget(null);
            EntityReferencePropertyManager.Next(behaviorTarget);
            text = EntityReferencePropertyManager.GetName();
            EntityReferencePropertyManager.SetBehaviorTarget(behaviorTarget); // put it back
        }
        else
        {
            return;
        }
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label(GUIIconSet.instance.target, alignedLabelStyle.Value, GUILayout.ExpandWidth(false));
        if (GUILayout.Button("<i>" + text + "</i>", GUILayout.ExpandWidth(false)))
        {
            BehaviorTargetPicker(GUIManager.guiGameObject, VoxelArrayEditor.instance,
                EntityReferencePropertyManager.CurrentEntity(), newValue =>
                {
                    property.value = newValue;
                });
        }
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    public static EntityPickerGUI BehaviorTargetPicker(GameObject guiGameObject, VoxelArrayEditor voxelArray,
        Entity self, Action<EntityBehavior.BehaviorTargetProperty> handler)
    {
        EntityPickerGUI entityPicker = guiGameObject.AddComponent<EntityPickerGUI>();
        entityPicker.voxelArray = voxelArray;
        entityPicker.allowNone = true;
        entityPicker.allowMultiple = false;
        entityPicker.allowNull = true;
        entityPicker.nullName = "Activators";
        entityPicker.handler = (ICollection<Entity> entities) =>
        {
            entityPicker = null;
            foreach (Entity entity in entities)
            {
                if (entity == null) // activator
                    handler(new EntityBehavior.BehaviorTargetProperty(new EntityReference(null), true));
                else if (entity == self)
                    handler(new EntityBehavior.BehaviorTargetProperty(new EntityReference(null), false));
                else
                    handler(new EntityBehavior.BehaviorTargetProperty(new EntityReference(entity), false));
                return;
            }
            handler(new EntityBehavior.BehaviorTargetProperty(new EntityReference(null), false));
        };
        return entityPicker;
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
            EntityPickerGUI picker = GUIManager.guiGameObject.AddComponent<EntityPickerGUI>();
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
                property.value = new EntityReference(null);
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
            FilterGUI filterGUI = GUIManager.guiGameObject.AddComponent<FilterGUI>();
            filterGUI.title = property.name + " by...";
            filterGUI.voxelArray = VoxelArrayEditor.instance;
            filterGUI.current = filter;
            filterGUI.handler = (ActivatedSensor.Filter newFilter) =>
            {
                property.value = newFilter;
            };
        }
        GUILayout.EndHorizontal();

        GUI.color = baseColor;
    }

    public static PropertyGUI Material(string materialDirectory, bool isOverlay = false,
        bool customTextureBase = false)
    {
        return (Property property) =>
        {
            GUILayout.BeginHorizontal();
            AlignedLabel(property);
            GUILayout.FlexibleSpace();
            // TODO: magic numbers
            RectOffset tagFieldStyleMargin = tagFieldStyle.Value.margin;
            Rect buttonRect = GUILayoutUtility.GetRect(150, 150);
            Rect textureRect = new Rect(
                buttonRect.xMin + 20, buttonRect.yMin + 20,
                buttonRect.width - 20 * 2, buttonRect.height - 20 * 2);
            if (GUI.Button(buttonRect, "  ", tagFieldStyle.Value))
            {
                MaterialSelectorGUI materialSelector
                    = GUIManager.guiGameObject.AddComponent<MaterialSelectorGUI>();
                materialSelector.title = "Change " + property.name;
                materialSelector.voxelArray = VoxelArrayEditor.instance;
                materialSelector.rootDirectory = materialDirectory;
                materialSelector.highlightMaterial = (Material)property.value;
                materialSelector.isOverlay = isOverlay;
                materialSelector.customTextureBase = customTextureBase;
                materialSelector.handler = (Material mat) =>
                {
                    property.setter(mat); // skip equality check, it could be the same material with a different color
                };
            }
            MaterialSelectorGUI.DrawMaterialTexture((Material)property.value,
                textureRect, isOverlay);
            GUILayout.EndHorizontal();
        };
    }

    public static void Texture(Property property)
    {
        GUILayout.BeginHorizontal();
        AlignedLabel(property);
        GUILayout.FlexibleSpace();
        // copied from Material()
        // TODO: magic numbers
        RectOffset tagFieldStyleMargin = tagFieldStyle.Value.margin;
        Rect buttonRect = GUILayoutUtility.GetRect(150, 150);
        Rect textureRect = new Rect(
            buttonRect.xMin + 20, buttonRect.yMin + 20,
            buttonRect.width - 20 * 2, buttonRect.height - 20 * 2);
        if (GUI.Button(buttonRect, "  ", tagFieldStyle.Value))
        {
            NativeGalleryWrapper.ImportTexture((Texture2D newTexture) => {
                if (newTexture != null)
                    property.setter(newTexture);  // skip equality check
            });
        }
        GUI.DrawTexture(textureRect, (Texture2D)property.value);
        GUILayout.EndHorizontal();
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
            ColorPickerGUI colorPicker = GUIManager.guiGameObject.AddComponent<ColorPickerGUI>();
            colorPicker.title = property.name;
            colorPicker.SetColor(valueColor);
            colorPicker.handler = (Color color) =>
            {
                property.value = color;
            };
        }
        GUI.color = baseColor;
    }

    public static void _TargetCustom(Property property,
        bool allowObjectTarget=true, bool allowVertical=true, bool alwaysWorld=false,
        bool allowRandom=true)
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
            TargetGUI targetGUI = GUIManager.guiGameObject.AddComponent<TargetGUI>();
            targetGUI.title = property.name;
            targetGUI.voxelArray = VoxelArrayEditor.instance;
            targetGUI.allowObjectTarget = allowObjectTarget;
            targetGUI.allowVertical = allowVertical;
            targetGUI.alwaysWorld = alwaysWorld;
            targetGUI.allowRandom = allowRandom;
            targetGUI.handler = (Target newTarget) =>
            {
                property.value = newTarget;
            };
        }
        GUILayout.EndHorizontal();

        GUI.color = baseColor;
    }

    public static void Target(Property property)
    {
        _TargetCustom(property);
    }

    public static void TargetWorldOnly(Property property)
    {
        _TargetCustom(property, alwaysWorld: true);
    }

    public static void Target4Directions(Property property)
    {
        _TargetCustom(property, allowObjectTarget: false, allowVertical: false,
            alwaysWorld: true, allowRandom: false);
    }

    public static void TargetStatic(Property property)
    {
        _TargetCustom(property, allowObjectTarget: false, allowRandom: false);
    }

    public static void TargetDirectionFilter(Property property)
    {
        var target = (Target)property.value;
        string targetString = target.ToString();

        if (target.entityRef.entity == null && target.direction == global::Target.NO_DIRECTION)
            targetString = "Any";

        GUILayout.BeginHorizontal();
        AlignedLabel(property);
        if (GUILayout.Button(targetString, GUI.skin.textField))
        {
            TargetGUI targetGUI = GUIManager.guiGameObject.AddComponent<TargetGUI>();
            targetGUI.title = property.name;
            targetGUI.voxelArray = VoxelArrayEditor.instance;
            targetGUI.allowNullTarget = true;
            targetGUI.allowObjectTarget = false;
            targetGUI.allowRandom = false;
            targetGUI.handler = (Target newTarget) =>
            {
                property.value = newTarget;
            };
        }
        GUILayout.EndHorizontal();
    }

    public static PropertyGUI EmbeddedData(EmbeddedDataType type, AudioPlayerFactory playerFactory = null)
    {
        return (Property property) =>
        {
            var embeddedData = (EmbeddedData)property.value;

            GUILayout.BeginHorizontal();
            AlignedLabel(property);
            if (GUILayout.Button(embeddedData.name, GUI.skin.textField))
            {
                var import = GUIManager.guiGameObject.AddComponent<DataImportGUI>();
                import.title = "Select " + property.name;
                import.type = type;
                import.playerFactory = playerFactory;
                import.dataAction = (data) =>
                {
                    property.value = data;
                };
            }
            GUILayout.EndHorizontal();
        };
    }
}
