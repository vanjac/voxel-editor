﻿using System.Collections.Generic;
using UnityEngine;

public class TargetGUI : GUIPanel {
    public System.Action<Target> handler;
    public VoxelArrayEditor voxelArray;
    public bool allowObjectTarget = true, allowNullTarget = false, allowVertical = true;
    public bool alwaysWorld = false, allowRandom = true;
    public string nullTargetName = StringSet.TargetAny;
    public Texture nullTargetIcon = IconSet.helpCircle;

    private int localState = 0;

    public override Rect GetRect(Rect safeRect, Rect screenRect) =>
        new Rect(GUIPanel.leftPanel.panelRect.xMax,
            GUIPanel.topPanel.panelRect.yMax, 880, 0);

    public override void WindowGUI() {
        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical();
        if (!alwaysWorld) {
            localState = GUILayout.SelectionGrid(localState,
                new string[] { StringSet.TargetWorld, StringSet.TargetLocal }, 2);
        }
        DirectionButtons();
        GUILayout.Space(16);  // fix weird layout issue
        GUILayout.EndVertical();

        GUILayout.Space(8);  // padding

        GUILayout.BeginVertical();
        GUILayout.FlexibleSpace();
        GUILayout.Space(8);  // additional padding
        GUILayout.Box("", GUIStyle.none, GUILayout.Width(200), GUILayout.Height(200));
        DrawCompass(this, GUILayoutUtility.GetLastRect());
        GUILayout.Space(8);
        GUILayout.FlexibleSpace();
        GUILayout.EndVertical();

        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        if (allowObjectTarget && GUILayout.Button(GUIUtils.PadContent(StringSet.TargetPickObject,
                IconSet.singleObject), StyleSet.buttonSmall)) {
            EntityPickerGUI picker = gameObject.AddComponent<EntityPickerGUI>();
            picker.voxelArray = voxelArray;
            picker.allowNone = false;
            picker.allowMultiple = false;
            picker.handler = (ICollection<Entity> entities) => {
                if (entities.Count > 0) {
                    foreach (Entity entity in entities) { // only first one
                        handler(new Target(entity));
                        return;
                    }
                }
            };
            Destroy(this);
        }
        if (allowNullTarget && GUILayout.Button(GUIUtils.PadContent(nullTargetName, nullTargetIcon),
                StyleSet.buttonSmall)) {
            handler(new Target(null));
            Destroy(this);
        }
        if (allowRandom && GUILayout.Button(
                GUIUtils.PadContent(StringSet.TargetRandom, IconSet.random), StyleSet.buttonSmall)) {
            handler(new Target(Target.RANDOM));  // don't check local state
            Destroy(this);
        }
        GUILayout.EndHorizontal();
    }

    private void DirectionButtons() {
        Color baseColor = GUI.color;
        GUILayout.BeginHorizontal();

        GUI.color = baseColor * new Color(0.2f, 0.2f, 1);
        GUILayout.BeginVertical();
        if (GUILayout.Button(StringSet.North, StyleSet.buttonSmall)) {
            SelectDirection(Target.NORTH);
        }
        if (GUILayout.Button(StringSet.South, StyleSet.buttonSmall)) {
            SelectDirection(Target.SOUTH);
        }
        GUILayout.EndVertical();

        GUI.color = baseColor * new Color(1, 0.2f, 0.2f);
        GUILayout.BeginVertical();
        if (GUILayout.Button(StringSet.East, StyleSet.buttonSmall)) {
            SelectDirection(Target.EAST);
        }
        if (GUILayout.Button(StringSet.West, StyleSet.buttonSmall)) {
            SelectDirection(Target.WEST);
        }
        GUILayout.EndVertical();

        if (allowVertical) {
            GUI.color = baseColor * new Color(0.2f, 1, 0.2f);
            GUILayout.BeginVertical();
            if (GUILayout.Button(StringSet.Up, StyleSet.buttonSmall)) {
                SelectDirection(Target.UP);
            }
            if (GUILayout.Button(StringSet.Down, StyleSet.buttonSmall)) {
                SelectDirection(Target.DOWN);
            }
            GUILayout.EndVertical();
        }

        GUILayout.EndHorizontal();
        GUI.color = baseColor;
    }

    private void SelectDirection(sbyte direction) {
        if (localState == 1) {
            direction |= Target.LOCAL_BIT;
        }
        handler(new Target(direction));
        Destroy(this);
    }

    public static void DrawCompass(GUIPanel panel, Rect rect) {
        float rotation = -Camera.main.transform.parent.rotation.eulerAngles.y;
        Matrix4x4 baseMatrix = GUI.matrix;
        panel.RotateAboutPoint(rect.center, rotation, Vector2.one);
        GUI.DrawTexture(rect, IconSet.compassLarge);
        GUI.matrix = baseMatrix;
    }
}