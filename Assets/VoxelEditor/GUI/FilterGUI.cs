using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FilterGUI : GUIPanel
{
    public delegate void FilterHandler(ActivatedSensor.Filter filter);

    public FilterHandler handler;
    public VoxelArrayEditor voxelArray;

    public override Rect GetRect(float width, float height)
    {
        return new Rect(width * .35f, height * .25f, width * .3f, 0);
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
            picker.items = GameScripts.entityFilterTypes;
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
            picker.items = GameScripts.behaviors;
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
