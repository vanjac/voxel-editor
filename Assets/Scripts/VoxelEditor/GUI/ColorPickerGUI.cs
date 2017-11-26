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

        panelRect = new Rect(190, 20, 180, 180);

        GUI.Box(panelRect, "Change Color");

        Rect paddedPanelRect = new Rect(panelRect.xMin + 10, panelRect.yMin + 25, panelRect.width - 20, panelRect.height - 25);
        GUILayout.BeginArea(paddedPanelRect);

        Color oldColor = color;

        GUILayout.BeginHorizontal();
        GUILayout.Label("R", GUILayout.ExpandWidth(false));
        color.r = GUILayout.HorizontalSlider(color.r, 0, 1);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("G", GUILayout.ExpandWidth(false));
        color.g = GUILayout.HorizontalSlider(color.g, 0, 1);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("B", GUILayout.ExpandWidth(false));
        color.b = GUILayout.HorizontalSlider(color.b, 0, 1);
        GUILayout.EndHorizontal();

        Texture2D solidColorTexture = new Texture2D(1, 1);
        solidColorTexture.SetPixel(0, 0, color);
        solidColorTexture.Apply();

        GUI.DrawTexture(new Rect(0, 80, 40, 40),
            solidColorTexture);

        GUILayout.EndArea();

        if (oldColor != color)
        {
            handler(color);
        }
    }
}
