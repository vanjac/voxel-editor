using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetGUI : GUIPanel
{
    public delegate void TargetHandler(Target target);

    public TargetHandler handler;
    public VoxelArrayEditor voxelArray;

    public override Rect GetRect(float width, float height)
    {
        return new Rect(width * .25f, height * .25f, width * .5f, 0);
    }

    public override void WindowGUI()
    {
        Color baseColor = GUI.color;
        GUILayout.BeginHorizontal();
        GUI.color = baseColor * Color.blue;
        if (GUILayout.Button("North"))
        {
            handler(new Target(5));
            Destroy(this);
        }
        GUI.color = baseColor * Color.red;
        if (GUILayout.Button("East"))
        {
            handler(new Target(1));
            Destroy(this);
        }
        GUI.color = baseColor * Color.green;
        if (GUILayout.Button("Up"))
        {
            handler(new Target(3));
            Destroy(this);
        }
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUI.color = baseColor * Color.blue;
        if (GUILayout.Button("South"))
        {
            handler(new Target(4));
            Destroy(this);
        }
        GUI.color = baseColor * Color.red;
        if (GUILayout.Button("West"))
        {
            handler(new Target(0));
            Destroy(this);
        }
        GUI.color = baseColor * Color.green;
        if (GUILayout.Button("Down"))
        {
            handler(new Target(2));
            Destroy(this);
        }
        GUILayout.EndHorizontal();
        GUI.color = baseColor;
        if (GUILayout.Button("Pick object"))
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
                        handler(new Target(entity));
                        return;
                    }
            };
            Destroy(this);
        }
    }
}