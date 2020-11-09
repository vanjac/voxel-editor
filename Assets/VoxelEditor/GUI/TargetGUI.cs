using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetGUI : GUIPanel
{
    public delegate void TargetHandler(Target target);

    public TargetHandler handler;
    public VoxelArrayEditor voxelArray;
    public bool allowObjectTarget = true, allowNullTarget = false, allowVertical = true;

    public override Rect GetRect(Rect safeRect, Rect screenRect)
    {
        return new Rect(GUIPanel.leftPanel.panelRect.xMax,
            GUIPanel.topPanel.panelRect.yMax, 960, 0);
    }

    public override void WindowGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        DirectionButtons();
        GUILayout.BeginHorizontal();
        if (allowObjectTarget && GUILayout.Button("Pick object"))
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
        if (allowNullTarget && GUILayout.Button("Any"))
        {
            handler(new Target(null));
            Destroy(this);
        }
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();

        GUILayout.BeginVertical();
        GUILayout.Label("North:");
        GUILayout.Box("", GUIStyle.none, GUILayout.Width(150), GUILayout.Height(150));
        float compassRotation = -Camera.main.transform.parent.rotation.eulerAngles.y;
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
            handler(new Target(Target.NORTH));
            Destroy(this);
        }
        if (GUILayout.Button("South"))
        {
            handler(new Target(Target.SOUTH));
            Destroy(this);
        }
        GUILayout.EndVertical();

        GUI.color = baseColor * Color.red;
        GUILayout.BeginVertical();
        if (GUILayout.Button("East"))
        {
            handler(new Target(Target.EAST));
            Destroy(this);
        }
        if (GUILayout.Button("West"))
        {
            handler(new Target(Target.WEST));
            Destroy(this);
        }
        GUILayout.EndVertical();

        if (allowVertical)
        {
            GUI.color = baseColor * Color.green;
            GUILayout.BeginVertical();
            if (GUILayout.Button("Up"))
            {
                handler(new Target(Target.UP));
                Destroy(this);
            }
            if (GUILayout.Button("Down"))
            {
                handler(new Target(Target.DOWN));
                Destroy(this);
            }
            GUILayout.EndVertical();
        }

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