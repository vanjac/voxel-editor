using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FilterGUI : GUIPanel
{
    public delegate void FilterHandler(ActivatedSensor.Filter filter);

    public ActivatedSensor.Filter current;
    public FilterHandler handler;
    public VoxelArrayEditor voxelArray;

    private static readonly System.Lazy<GUIStyle> buttonStyle = new System.Lazy<GUIStyle>(() =>
    {
        var style = new GUIStyle(GUIStyleSet.instance.buttonLarge);
        style.alignment = TextAnchor.MiddleLeft;
        return style;
    });

    public override Rect GetRect(Rect safeRect, Rect screenRect)
    {
        return new Rect(GUIPanel.leftPanel.panelRect.xMax,
            GUIPanel.topPanel.panelRect.yMax, 500, 0);
    }

    public override void WindowGUI()
    {
        if (GUILayout.Button(GUIUtils.MenuContent("Specific object", GUIIconSet.instance.singleObject),
            buttonStyle.Value))
        {
            EntityPickerGUI picker = gameObject.AddComponent<EntityPickerGUI>();
            picker.voxelArray = voxelArray;
            picker.allowNone = false;
            picker.allowMultiple = false;
            picker.handler = (ICollection<Entity> entities) =>
            {
                if (entities.Count > 0)
                    foreach (Entity entity in entities) // only first one
                    {
                        handler(new ActivatedSensor.EntityFilter(entity));
                        return;
                    }
            };
            Destroy(this);
        }
        if (GUILayout.Button(GUIUtils.MenuContent("Object type", GUIIconSet.instance.objectType),
            buttonStyle.Value))
        {
            TypePickerGUI picker = gameObject.AddComponent<TypePickerGUI>();
            picker.title = "Filter by object type";
            picker.categories = new PropertiesObjectType[][] { GameScripts.entityFilterTypes };
            picker.handler = (PropertiesObjectType type) =>
            {
                handler(new ActivatedSensor.EntityTypeFilter(type));
            };
            Destroy(this);
        }
        if (GUILayout.Button(GUIUtils.MenuContent("Active behavior", GUIIconSet.instance.behavior),
            buttonStyle.Value))
        {
            TypePickerGUI picker = gameObject.AddComponent<TypePickerGUI>();
            picker.title = "Filter by active behavior";
            picker.categoryNames = GameScripts.behaviorTabNames;
            picker.categories = GameScripts.behaviorTabs;
            picker.handler = (PropertiesObjectType type) =>
            {
                handler(new ActivatedSensor.EntityTypeFilter(type));
            };
            Destroy(this);
        }
        if (GUILayout.Button(GUIUtils.MenuContent("Tags", GUIIconSet.instance.entityTag),
            buttonStyle.Value))
        {
            TagPickerGUI picker = gameObject.AddComponent<TagPickerGUI>();
            picker.title = "Filter by tags";
            picker.multiple = true;
            if (current is ActivatedSensor.MultipleTagFilter)
                picker.multiSelection = (current as ActivatedSensor.MultipleTagFilter).tagBits;
            else if (current is ActivatedSensor.TagFilter)
                picker.multiSelection = (byte)(1 << (current as ActivatedSensor.TagFilter).tag);
            picker.handler = (byte tagBits) =>
            {
                handler(new ActivatedSensor.MultipleTagFilter(tagBits));
            };
            Destroy(this);
        }
    }
}
