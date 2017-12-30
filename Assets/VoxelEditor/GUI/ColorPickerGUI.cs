using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ColorPickerGUI : GUIPanel
{
    public delegate void ColorChangeHandler(Color color);

    private Color color;
    private float hue, saturation, value;
    private Texture2D colorTexture = null;
    private Texture2D hueTexture, saturationTexture, valueTexture;
    private GUIStyle hueSliderStyle = null, saturationSliderStyle = null, valueSliderStyle = null;
    public ColorChangeHandler handler;

    public override Rect GetRect(float width, float height)
    {
        return new Rect(height * .55f, height * .1f, width * .4f, 0);
    }

    public override string GetName()
    {
        return "Change Color";
    }

    public void SetColor(Color c)
    {
        color = c;
        Color.RGBToHSV(c, out hue, out saturation, out value);
        UpdateTexture();
    }

    private void UpdateTexture()
    {
        if (colorTexture == null)
            colorTexture = new Texture2D(1, 1);
        colorTexture.SetPixel(0, 0, color);
        colorTexture.Apply();

        if (hueSliderStyle != null)
        {
            if(hueTexture == null)
                hueTexture = new Texture2D(256, 1);
            for (int x = 0; x < hueTexture.width; x++)
                hueTexture.SetPixel(x, 0, Color.HSVToRGB(x / 256.0f, saturation, value));
            hueTexture.Apply();
            hueSliderStyle.normal.background = hueTexture;
        }

        if (saturationSliderStyle != null)
        {
            if (saturationTexture == null)
                saturationTexture = new Texture2D(256, 1);
            for (int x = 0; x < saturationTexture.width; x++)
                saturationTexture.SetPixel(x, 0, Color.HSVToRGB(hue, x / 256.0f, value));
            saturationTexture.Apply();
            saturationSliderStyle.normal.background = saturationTexture;
        }

        if (valueSliderStyle != null)
        {
            if (valueTexture == null)
                valueTexture = new Texture2D(256, 1);
            for (int x = 0; x < valueTexture.width; x++)
                valueTexture.SetPixel(x, 0, Color.HSVToRGB(hue, saturation, x / 256.0f));
            valueTexture.Apply();
            valueSliderStyle.normal.background = valueTexture;
        }
    }

    public override void WindowGUI()
    {
        if (hueSliderStyle == null)
        {
            hueSliderStyle = NewColorSliderStyle();
            saturationSliderStyle = NewColorSliderStyle();
            valueSliderStyle = NewColorSliderStyle();
            UpdateTexture();
        }

        float oldHue = hue, oldSaturation = saturation, oldValue = value;

        GUILayout.Label("Hue:");
        hue = GUILayout.HorizontalSlider(hue, 0, 1, hueSliderStyle, GUI.skin.horizontalSliderThumb);

        GUILayout.Label("Saturation:");
        saturation = GUILayout.HorizontalSlider(saturation, 0, 1, saturationSliderStyle, GUI.skin.horizontalSliderThumb);

        GUILayout.Label("Brightness:");
        value = GUILayout.HorizontalSlider(value, 0, 1, valueSliderStyle, GUI.skin.horizontalSliderThumb);

        if (oldHue != hue || oldSaturation != saturation || oldValue != value)
        {
            color = Color.HSVToRGB(hue, saturation, value);
            handler(color);
            UpdateTexture();
        }

        Rect colorRect = GUILayoutUtility.GetAspectRect(4.0f);

        GUI.DrawTexture(colorRect, colorTexture);
    }

    private GUIStyle NewColorSliderStyle()
    {
        GUIStyle style = new GUIStyle(GUI.skin.horizontalSlider);
        style.border = new RectOffset();
        return style;
    }
}
