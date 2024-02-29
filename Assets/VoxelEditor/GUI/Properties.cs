using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

public static class PropertyGUIs
{
    private static GUIStringSet StringSet => GUIPanel.StringSet;

    private static TouchScreenKeyboard numberKeyboard = null;
    private static Action<string> keyboardHandler;

    private static readonly Lazy<GUIStyle> alignedLabelStyle = new Lazy<GUIStyle>(() =>
    {
        var style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.MiddleLeft;
        style.padding.right = 0;
        return style;
    });
    private static readonly Lazy<GUIStyle> headerLabelStyle = new Lazy<GUIStyle>(() =>
    {
        var style = new GUIStyle(GUI.skin.label);
        style.padding.top = 0;
        style.padding.bottom = 0;
        return style;
    });
    private static readonly Lazy<GUIStyle> inlineLabelStyle = new Lazy<GUIStyle>(() =>
    {
        var style = new GUIStyle(alignedLabelStyle.Value);
        style.padding.left = 0;
        return style;
    });
    private static readonly Lazy<GUIStyle> tagFieldStyle = new Lazy<GUIStyle>(() =>
    {
        var style = new GUIStyle(GUI.skin.textField);
        style.fontSize = GUI.skin.font.fontSize * 2;
        return style;
    });

    public static void AlignedLabel(Property property)
    {
        var name = property.name(StringSet);
        if (name != "")
            GUILayout.Label(name, alignedLabelStyle.Value, GUILayout.ExpandWidth(false));
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
        string[] names = property.name(StringSet).Split('|');
        var values = ((bool, bool))property.value;
        var buttonStyle = GUIPanel.StyleSet.buttonTab;
        GUILayout.BeginHorizontal();
        values.Item1 ^= GUIUtils.HighlightedButton(names[0], buttonStyle, values.Item1);
        values.Item2 ^= GUIUtils.HighlightedButton(names[1], buttonStyle, values.Item2);
        property.value = values;
        GUILayout.EndHorizontal();
    }

    public static void Enum(Property property)
    {
        System.Enum e = (System.Enum)property.value;
        var buttonStyle = GUIPanel.StyleSet.buttonTab;
        GUILayout.BeginHorizontal();
        foreach (var enumValue in System.Enum.GetValues(e.GetType()))
        {
            // TODO: localize!
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

    private static bool ParseFloat(string s, out float result)
    {
        // do NOT allow thousands separators, they can get confused for decimals
        if (float.TryParse(s, NumberStyles.Float, CultureInfo.CurrentCulture, out result))
            return true;
        // a bug on Android prevents commas from being used as decimal separators
        if (float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out result))
            return true;
        return false;
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
                numberKeyboard.selection = new RangeInt(0, sValue.Length);
                keyboardHandler = text =>
                {
                    if (ParseFloat(text, out float newValue))
                        property.value = newValue;
                };
            }
        }
        else // TouchScreenKeyboard not supported
        {
            sValue = GUILayout.TextField(sValue);
            if (ParseFloat(sValue, out float newValue))
                property.value = newValue;
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
        GUILayout.Label(property.name(StringSet) + ":", headerLabelStyle.Value);
        GUILayout.BeginHorizontal();
        var range = ((float, float))property.value;
        Property wrapper1 = new Property(
            property.id + "1",
            GUIStringSet.Empty,
            () => range.Item1,
            v => property.value = ((float)v, range.Item2),
            PropertyGUIs.Empty);
        Float(wrapper1);
        GUILayout.Label(separator, inlineLabelStyle.Value, GUILayout.ExpandWidth(false));
        Property wrapper2 = new Property(
            property.id + "2",
            GUIStringSet.Empty,
            () => range.Item2,
            v => property.value = (range.Item1, (float)v),
            PropertyGUIs.Empty);
        Float(wrapper2);
        GUILayout.EndHorizontal();
    }

    public static void FloatRange(Property property)
    {
        FloatPair(property, StringSet.RangeSeparator);
    }

    public static void FloatDimensions(Property property)
    {
        FloatPair(property, StringSet.DimensionSeparator);
    }

    public static void Vector3(Property property)
    {
        GUILayout.Label(property.name(StringSet) + ":", headerLabelStyle.Value);
        GUILayout.BeginHorizontal();
        var vec = (Vector3)property.value;
        Color baseColor = GUI.color;

        GUI.color = baseColor * new Color(1, 0.6f, 0.6f);
        Property wrapperX = new Property("", GUIStringSet.Empty,
            () => vec.x,
            v => property.value = new Vector3((float)v, vec.y, vec.z),
            PropertyGUIs.Empty);
        Float(wrapperX);

        GUI.color = baseColor * new Color(0.6f, 1, 0.6f);
        Property wrapperY = new Property("", GUIStringSet.Empty,
            () => vec.y,
            v => property.value = new Vector3(vec.x, (float)v, vec.z),
            PropertyGUIs.Empty);
        Float(wrapperY);

        GUI.color = baseColor * new Color(0.6f, 0.6f, 1);
        Property wrapperZ = new Property("", GUIStringSet.Empty,
            () => vec.z,
            v => property.value = new Vector3(vec.x, vec.y, (float)v),
            PropertyGUIs.Empty);
        Float(wrapperZ);

        GUI.color = baseColor;
        GUILayout.EndHorizontal();
    }

    public static void Tag(Property property)
    {
        string tagString = Entity.TagToString((byte)property.value);
        GUILayout.BeginHorizontal();
        AlignedLabel(property);
        GUILayout.FlexibleSpace();
        if (GUILayout.Button(" " + tagString + " ", tagFieldStyle.Value, GUILayout.ExpandWidth(false)))
        {
            TagPickerGUI picker = GUIPanel.GuiGameObject.AddComponent<TagPickerGUI>();
            picker.title = StringSet.ChangeProperty(property.name(StringSet));
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
        GUILayout.Label(StringSet.SensorConditionHeader);
        TutorialGUI.TutorialHighlight("behavior condition");
        property.value = (EntityBehavior.Condition)GUILayout.SelectionGrid((int)condition,
            new string[] { StringSet.SensorOn, StringSet.SensorOff, StringSet.SensorBoth }, 3,
            GUIPanel.StyleSet.buttonTab);
        TutorialGUI.ClearHighlight();
    }

    public static void ActivatorBehaviorCondition(Property property)
    {
        GUILayout.Label(StringSet.WhenSensorIsOn);
        property.value = EntityBehavior.Condition.ON;
    }

    public static void BehaviorTarget(Property property)
    {
        var value = (EntityBehavior.BehaviorTargetProperty)(property.value);
        Entity behaviorTarget = value.targetEntity.entity;
        string text;
        if (value.targetEntityIsActivator)
        {
            text = StringSet.EntityActivators;
        }
        else if (behaviorTarget != null)
        {
            // only temporarily, so the name won't be "Target":
            EntityReferencePropertyManager.SetBehaviorTarget(null);
            EntityReferencePropertyManager.Next(behaviorTarget);
            text = EntityReferencePropertyManager.GetName(StringSet);
            EntityReferencePropertyManager.SetBehaviorTarget(behaviorTarget); // put it back
        }
        else
        {
            return;
        }
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label(GUIPanel.IconSet.target, alignedLabelStyle.Value, GUILayout.ExpandWidth(false));
        if (GUILayout.Button("<i>" + text + "</i>", GUILayout.ExpandWidth(false)))
        {
            BehaviorTargetPicker(GUIPanel.GuiGameObject, VoxelArrayEditor.instance,
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
        entityPicker.nullName = StringSet.EntityActivators;
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
        _EntityReferenceCustom(property, false, StringSet.EntityRefNone);
    }

    public static void EntityReferenceWithNull(Property property)
    {
        _EntityReferenceCustom(property, true, StringSet.EntityRefNone);
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
            valueString = EntityReferencePropertyManager.GetName(StringSet);
        }

        GUILayout.BeginHorizontal();
        AlignedLabel(property);
        if (GUILayout.Button(valueString, GUI.skin.textField))
        {
            EntityPickerGUI picker = GUIPanel.GuiGameObject.AddComponent<EntityPickerGUI>();
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
        string filterString = filter.ToString(StringSet);

        Color baseColor = GUI.color;
        ActivatedSensor.EntityFilter entityFilter = filter as ActivatedSensor.EntityFilter;
        if (entityFilter != null)
        {
            Entity e = entityFilter.entityRef.entity;
            if (e != null)
            {
                EntityReferencePropertyManager.Next(e);
                GUI.color = baseColor * EntityReferencePropertyManager.GetColor();
                filterString = EntityReferencePropertyManager.GetName(StringSet);
            }
        }

        GUILayout.BeginHorizontal();
        AlignedLabel(property);
        if (GUILayout.Button(filterString, GUI.skin.textField))
        {
            FilterGUI filterGUI = GUIPanel.GuiGameObject.AddComponent<FilterGUI>();
            filterGUI.title = StringSet.FilterByTitle;
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
        bool customTextureBase = false) =>
        (Property property) =>
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
                    = GUIPanel.GuiGameObject.AddComponent<MaterialSelectorGUI>();
                materialSelector.title = StringSet.ChangeProperty(property.name(StringSet));
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
            NativeGalleryWrapper.ImportTexture((Texture2D newTexture) =>
            {
                if (newTexture != null)
                    property.setter(newTexture);  // skip equality check
            });
        }
        GUI.DrawTexture(textureRect, (Texture2D)property.value);
        GUILayout.EndHorizontal();
    }

    public static PropertyGUI Slider(float minValue, float maxValue) =>
        (Property property) =>
        {
            GUILayout.Label(property.name(StringSet) + ":");
            property.value = GUILayout.HorizontalSlider(
                (float)property.value, minValue, maxValue);
        };

    public static void Color(Property property)
    {
        Color baseColor = GUI.color;
        Color valueColor = (Color)property.value;
        GUI.color = baseColor * valueColor;
        var name = property.name(StringSet);
        if (GUILayout.Button(name))
        {
            ColorPickerGUI colorPicker = GUIPanel.GuiGameObject.AddComponent<ColorPickerGUI>();
            colorPicker.title = name;
            colorPicker.SetColor(valueColor);
            colorPicker.handler = (Color color) =>
            {
                property.value = color;
            };
        }
        GUI.color = baseColor;
    }

    public static void _TargetCustom(Property property,
        bool allowObjectTarget = true, bool allowVertical = true, bool alwaysWorld = false,
        bool allowRandom = true)
    {
        var target = (Target)property.value;
        string targetString = target.ToString(StringSet);

        Color baseColor = GUI.color;
        if (target.entityRef.entity != null)
        {
            EntityReferencePropertyManager.Next(target.entityRef.entity);
            GUI.color = baseColor * EntityReferencePropertyManager.GetColor();
            targetString = EntityReferencePropertyManager.GetName(StringSet);
        }

        GUILayout.BeginHorizontal();
        AlignedLabel(property);
        if (GUILayout.Button(targetString, GUI.skin.textField))
        {
            TargetGUI targetGUI = GUIPanel.GuiGameObject.AddComponent<TargetGUI>();
            targetGUI.title = property.name(StringSet);
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

    public static void TargetFacing(Property property)
    {
        var target = (Target)property.value;
        string targetString = target.ToString(StringSet);

        if (target.entityRef.entity == null && target.direction == global::Target.NO_DIRECTION)
            targetString = StringSet.Camera;

        GUILayout.BeginHorizontal();
        AlignedLabel(property);
        if (GUILayout.Button(targetString, GUI.skin.textField))
        {
            TargetGUI targetGUI = GUIPanel.GuiGameObject.AddComponent<TargetGUI>();
            targetGUI.title = property.name(StringSet);
            targetGUI.voxelArray = VoxelArrayEditor.instance;
            targetGUI.allowNullTarget = true;
            targetGUI.allowObjectTarget = false;
            targetGUI.allowRandom = false;
            targetGUI.allowVertical = false;
            targetGUI.nullTargetName = StringSet.Camera;
            targetGUI.nullTargetIcon = GUIPanel.IconSet.camera;
            targetGUI.handler = (Target newTarget) =>
            {
                property.value = newTarget;
            };
        }
        GUILayout.EndHorizontal();
    }

    public static void TargetDirectionFilter(Property property)
    {
        var target = (Target)property.value;
        string targetString = target.ToString(StringSet);

        if (target.entityRef.entity == null && target.direction == global::Target.NO_DIRECTION)
            targetString = StringSet.TargetAny;

        GUILayout.BeginHorizontal();
        AlignedLabel(property);
        if (GUILayout.Button(targetString, GUI.skin.textField))
        {
            TargetGUI targetGUI = GUIPanel.GuiGameObject.AddComponent<TargetGUI>();
            targetGUI.title = property.name(StringSet);
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

    public static void PivotProp(Property property)
    {
        var pivot = (Pivot)property.value;
        string pivotString;
        if (pivot.x == Pivot.Pos.Center && pivot.y == Pivot.Pos.Center && pivot.z == Pivot.Pos.Center)
            pivotString = StringSet.Center;
        else
        {
            pivotString = (int)pivot.y switch
                {0 => StringSet.Bottom + " ", 2 => StringSet.Top + " ", _ => ""};
            if (pivot.x == Pivot.Pos.Center || pivot.z == Pivot.Pos.Center)
            {
                pivotString += (int)pivot.z switch
                    {0 => StringSet.South, 2 => StringSet.North, _ => ""};
                pivotString += (int)pivot.x switch
                    {0 => StringSet.West, 2 => StringSet.East, _ => ""};
            }
            else
            {
                pivotString += (int)pivot.z switch
                    {0 => StringSet.SouthLetter, 2 => StringSet.NorthLetter, _ => ""};
                pivotString += (int)pivot.x switch
                    {0 => StringSet.WestLetter, 2 => StringSet.EastLetter, _ => ""};
            }
        }

        GUILayout.BeginHorizontal();
        AlignedLabel(property);
        if (GUILayout.Button(pivotString, GUI.skin.textField))
        {
            PivotGUI pivotGUI = GUIPanel.GuiGameObject.AddComponent<PivotGUI>();
            pivotGUI.title = property.name(StringSet);
            pivotGUI.value = pivot;
            pivotGUI.handler = (Pivot newPivot) => { property.value = newPivot; };
        }
        GUILayout.EndHorizontal();
    }

    public static PropertyGUI EmbeddedData(EmbeddedDataType type, AudioPlayerFactory playerFactory = null) =>
        (Property property) =>
        {
            var embeddedData = (EmbeddedData)property.value;

            GUILayout.BeginHorizontal();
            AlignedLabel(property);
            if (GUILayout.Button(embeddedData.name, GUI.skin.textField))
            {
                var import = GUIPanel.GuiGameObject.AddComponent<DataImportGUI>();
                import.title = StringSet.SelectProperty(property.name(StringSet));
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
