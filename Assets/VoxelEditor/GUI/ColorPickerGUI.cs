using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ColorPickerGUI : GUIPanel
{
    public delegate void ColorChangeHandler(Color color);

    public Color color = Color.red;
    public ColorChangeHandler handler;

    public override Rect GetRect(float width, float height)
    {
        return new Rect(height * .55f, height * .1f, height / 2, 0);
    }

    public override string GetName()
    {
        return "Change Color";
    }

    public override void WindowGUI()
    {
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

        Rect colorRect = GUILayoutUtility.GetAspectRect(3.0f);

        GUI.DrawTexture(colorRect,
            solidColorTexture);

        if (oldColor != color)
        {
            handler(color);
        }
    }
}
