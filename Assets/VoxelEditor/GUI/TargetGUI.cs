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
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        DirectionButtons();
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
        GUILayout.EndVertical();

        GUILayout.BeginVertical();
        GUILayout.Label("North:");
        GUILayout.Box("", GUIStyle.none, GUILayout.Width(150), GUILayout.Height(150));
        float compassRotation = -Camera.current.transform.parent.rotation.eulerAngles.y;
        DrawCompass(GUILayoutUtility.GetLastRect(), compassRotation);
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
    }

    private void DirectionButtons()
    {
        Color baseColor = GUI.color;
        GUILayout.BeginHorizontal();

        GUI.color = baseColor * Color.blue;
        GUILayout.BeginVertical();
        if (GUILayout.Button("North"))
        {
            handler(new Target(5));
            Destroy(this);
        }
        if (GUILayout.Button("South"))
        {
            handler(new Target(4));
            Destroy(this);
        }
        GUILayout.EndVertical();

        GUI.color = baseColor * Color.red;
        GUILayout.BeginVertical();
        if (GUILayout.Button("East"))
        {
            handler(new Target(1));
            Destroy(this);
        }
        if (GUILayout.Button("West"))
        {
            handler(new Target(0));
            Destroy(this);
        }
        GUILayout.EndVertical();

        GUI.color = baseColor * Color.green;
        GUILayout.BeginVertical();
        if (GUILayout.Button("Up"))
        {
            handler(new Target(3));
            Destroy(this);
        }
        if (GUILayout.Button("Down"))
        {
            handler(new Target(2));
            Destroy(this);
        }
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();
        GUI.color = baseColor;
    }

    private void DrawCompass(Rect rect, float rotation)
    {
        Matrix4x4 baseMatrix = GUI.matrix;
        RotateAboutPoint(rect.center, rotation, Vector2.one);
        GUI.DrawTexture(rect, GUIIconSet.instance.compass);
        GUI.matrix = baseMatrix;
    }
}