using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIUtils
{
    public static readonly Lazy<GUIStyle> LABEL_WORD_WRAPPED = new Lazy<GUIStyle>(() =>
    {
        var style = new GUIStyle(GUI.skin.label);
        style.wordWrap = true;
        return style;
    });

    public static readonly Lazy<GUIStyle> LABEL_CENTERED = new Lazy<GUIStyle>(() =>
    {
        var style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.MiddleCenter;
        return style;
    });

    public static readonly Lazy<GUIStyle> LABEL_HORIZ_CENTERED = new Lazy<GUIStyle>(() =>
    {
        var style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.UpperCenter;
        return style;
    });

    public static bool HighlightedButton(string text, GUIStyle style = null, bool highlight = true, params GUILayoutOption[] options)
    {
        if (style == null)
            style = GUI.skin.button;
        if (highlight)
            return !GUILayout.Toggle(true, text, style, options);
        else
            return GUILayout.Button(text, style, options);
    }

    public static bool HighlightedButton(Texture image, GUIStyle style = null, bool highlight = true, params GUILayoutOption[] options)
    {
        if (style == null)
            style = GUI.skin.button;
        if (highlight)
            return !GUILayout.Toggle(true, image, style, options);
        else
            return GUILayout.Button(image, style, options);
    }

    public static bool HighlightedButton(GUIContent content, GUIStyle style = null, bool highlight = true, params GUILayoutOption[] options)
    {
        if (style == null)
            style = GUI.skin.button;
        if (highlight)
            return !GUILayout.Toggle(true, content, style, options);
        else
            return GUILayout.Button(content, style, options);
    }

    public static void BeginButtonHorizontal(string name, GUIStyle style = null)
    {
        if (style == null)
            style = GUI.skin.button;
        GUILayout.BeginHorizontal(style);
    }

    public static bool EndButtonHorizontal(string name)
    {
        GUILayout.EndHorizontal();
        return EndButtonGroup(name);
    }

    public static void BeginButtonVertical(string name, GUIStyle style = null)
    {
        if (style == null)
            style = GUI.skin.button;
        GUILayout.BeginVertical(style);
    }

    public static bool EndButtonVertical(string name)
    {
        GUILayout.EndVertical();
        return EndButtonGroup(name);
    }

    private static bool EndButtonGroup(string name)
    {
        Rect buttonRect = GUILayoutUtility.GetLastRect();
        return GUI.Button(buttonRect, "", GUIStyle.none);
    }
}
