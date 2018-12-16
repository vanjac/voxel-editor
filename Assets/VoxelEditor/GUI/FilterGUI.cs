using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FilterGUI : GUIPanel
{
    public delegate void FilterHandler(ActivatedSensor.Filter filter);

    public FilterHandler handler;
    public VoxelArrayEditor voxelArray;

    public override Rect GetRect(Rect safeRect, Rect screenRect)
    {
        return new Rect(GUIPanel.leftPanel.panelRect.xMax,
            GUIPanel.topPanel.panelRect.yMax, 576, 0);
    }

    public override void WindowGUI()
    {
        if (GUILayout.Button("Specific object"))
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
        if (GUILayout.Button("Object type"))
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
        if (GUILayout.Button("Active behavior type"))
        {
            TypePickerGUI picker = gameObject.AddComponent<TypePickerGUI>();
            picker.title = "Filter by behavior type";
            picker.categoryNames = GameScripts.behaviorTabNames;
            picker.categories = GameScripts.behaviorTabs;
            picker.handler = (PropertiesObjectType type) =>
            {
                handler(new ActivatedSensor.EntityTypeFilter(type));
            };
            Destroy(this);
        }
        if (GUILayout.Button("Tag"))
        {
            TagPickerGUI picker = gameObject.AddComponent<TagPickerGUI>();
            picker.title = "Filter by tag";
            picker.handler = (byte tag) =>
            {
                handler(new ActivatedSensor.TagFilter(tag));
            };
            Destroy(this);
        }
    }
}
