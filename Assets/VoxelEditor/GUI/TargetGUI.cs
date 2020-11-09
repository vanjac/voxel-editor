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
        GUILayout.Box("", GUIStyle.none, GUILayout.Height(16));  // fix weird layout issue
        GUILayout.EndVertical();

        GUILayout.Box("", GUIStyle.none, GUILayout.Width(200), GUILayout.Height(200));
        float compassRotation = -Camera.main.transform.parent.rotation.eulerAngles.y;
        DrawCompass(GUILayoutUtility.GetLastRect(), compassRotation);
        GUILayout.EndHorizontal();
    }

    private void DirectionButtons()
    {
        Color baseColor = GUI.color;
        GUILayout.BeginHorizontal();

        GUI.color = baseColor * new Color(0.2f, 0.2f, 1);
        GUILayout.BeginVertical();
        if (GUILayout.Button("North", GUIStyleSet.instance.buttonSmall))
        {
            handler(new Target(Target.NORTH));
            Destroy(this);
        }
        if (GUILayout.Button("South", GUIStyleSet.instance.buttonSmall))
        {
            handler(new Target(Target.SOUTH));
            Destroy(this);
        }
        GUILayout.EndVertical();

        GUI.color = baseColor * new Color(1, 0.2f, 0.2f);
        GUILayout.BeginVertical();
        if (GUILayout.Button("East", GUIStyleSet.instance.buttonSmall))
        {
            handler(new Target(Target.EAST));
            Destroy(this);
        }
        if (GUILayout.Button("West", GUIStyleSet.instance.buttonSmall))
        {
            handler(new Target(Target.WEST));
            Destroy(this);
        }
        GUILayout.EndVertical();

        if (allowVertical)
        {
            GUI.color = baseColor * new Color(0.2f, 1, 0.2f);
            GUILayout.BeginVertical();
            if (GUILayout.Button("Up", GUIStyleSet.instance.buttonSmall))
            {
                handler(new Target(Target.UP));
                Destroy(this);
            }
            if (GUILayout.Button("Down", GUIStyleSet.instance.buttonSmall))
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
        GUI.DrawTexture(rect, GUIIconSet.instance.compassLarge);
        GUI.matrix = baseMatrix;
    }
}