using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ColorPickerGUI : GUIPanel
{
    public delegate void ColorChangeHandler(Color color);

    public Color color = Color.red;
    public ColorChangeHandler handler;

    public override void OnEnable()
    {
        depth = -1;
        base.OnEnable();
    }

    public override void OnGUI()
    {
        base.OnGUI();

        panelRect = new Rect(targetHeight * .55f, targetHeight * .1f, targetHeight / 2, targetHeight / 2);
        GUILayout.BeginArea(panelRect, GUI.skin.box);

        GUI.skin.label.alignment = TextAnchor.UpperCenter;
        GUILayout.Label("Change Color");
        GUI.skin.label.alignment = TextAnchor.UpperLeft;

        Color oldColor = color;

        GUILayout.BeginHorizontal();
        GUILayout.Label("R ", GUILayout.ExpandWidth(false));
        color.r = GUILayout.HorizontalSlider(color.r, 0, 1);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("G ", GUILayout.ExpandWidth(false));
        color.g = GUILayout.HorizontalSlider(color.g, 0, 1);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("B ", GUILayout.ExpandWidth(false));
        color.b = GUILayout.HorizontalSlider(color.b, 0, 1);
        GUILayout.EndHorizontal();

        Texture2D solidColorTexture = new Texture2D(1, 1);
        solidColorTexture.SetPixel(0, 0, color);
        solidColorTexture.Apply();

        Rect colorRect = GUILayoutUtility.GetAspectRect(1.0f);

        GUI.DrawTexture(colorRect,
            solidColorTexture);

        GUILayout.EndArea();

        if (oldColor != color)
        {
            handler(color);
        }
    }
}
