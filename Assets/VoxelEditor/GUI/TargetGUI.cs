using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetGUI : GUIPanel
{
    public delegate void TargetHandler(Target target);

    public TargetHandler handler;
    public VoxelArrayEditor voxelArray;
    public bool allowObjectTarget = true, allowNullTarget = false, allowVertical = true;
    public bool alwaysWorld = false;

    private int localState = 0;

    public override Rect GetRect(Rect safeRect, Rect screenRect)
    {
        return new Rect(GUIPanel.leftPanel.panelRect.xMax,
            GUIPanel.topPanel.panelRect.yMax, 880, 0);
    }

    public override void WindowGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        if (!alwaysWorld)
            localState = GUILayout.SelectionGrid(localState, new string[] {"World", "Local"}, 2);
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

        GUILayout.Box("", GUIStyle.none, GUILayout.Width(8));  // padding
        GUILayout.Box("", GUIStyle.none, GUILayout.Width(200), GUILayout.Height(200));
        float compassRotation = -Camera.main.transform.parent.rotation.eulerAngles.y;
        DrawCompass(GUILayoutUtility.GetLastRect(), compassRotation);
        GUILayout.Box("", GUIStyle.none, GUILayout.Width(8));  // padding
        GUILayout.EndHorizontal();
    }

    private void DirectionButtons()
    {
        Color baseColor = GUI.color;
        GUILayout.BeginHorizontal();

        GUI.color = baseColor * new Color(0.2f, 0.2f, 1);
        GUILayout.BeginVertical();
        if (GUILayout.Button("North", GUIStyleSet.instance.buttonSmall))
            SelectDirection(Target.NORTH);
        if (GUILayout.Button("South", GUIStyleSet.instance.buttonSmall))
            SelectDirection(Target.SOUTH);
        GUILayout.EndVertical();

        GUI.color = baseColor * new Color(1, 0.2f, 0.2f);
        GUILayout.BeginVertical();
        if (GUILayout.Button("East", GUIStyleSet.instance.buttonSmall))
            SelectDirection(Target.EAST);
        if (GUILayout.Button("West", GUIStyleSet.instance.buttonSmall))
            SelectDirection(Target.WEST);
        GUILayout.EndVertical();

        if (allowVertical)
        {
            GUI.color = baseColor * new Color(0.2f, 1, 0.2f);
            GUILayout.BeginVertical();
            if (GUILayout.Button("Up", GUIStyleSet.instance.buttonSmall))
                SelectDirection(Target.UP);
            if (GUILayout.Button("Down", GUIStyleSet.instance.buttonSmall))
                SelectDirection(Target.DOWN);
            GUILayout.EndVertical();
        }

        GUILayout.EndHorizontal();
        GUI.color = baseColor;
    }

    private void SelectDirection(sbyte direction)
    {
        if (localState == 1)
            direction |= Target.LOCAL_BIT;
        handler(new Target(direction));
        Destroy(this);
    }

    private void DrawCompass(Rect rect, float rotation)
    {
        Matrix4x4 baseMatrix = GUI.matrix;
        RotateAboutPoint(rect.center, rotation, Vector2.one);
        GUI.DrawTexture(rect, GUIIconSet.instance.compassLarge);
        GUI.matrix = baseMatrix;
    }
}