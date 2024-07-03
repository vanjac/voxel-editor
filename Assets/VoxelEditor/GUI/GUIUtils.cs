using UnityEngine;

public static class GUIUtils {
    public static readonly System.Lazy<GUIStyle> LABEL_WORD_WRAPPED = new System.Lazy<GUIStyle>(() => {
        var style = new GUIStyle(GUI.skin.label);
        style.wordWrap = true;
        return style;
    });

    public static readonly System.Lazy<GUIStyle> LABEL_CENTERED = new System.Lazy<GUIStyle>(() => {
        var style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.MiddleCenter;
        return style;
    });

    public static readonly System.Lazy<GUIStyle> LABEL_HORIZ_CENTERED = new System.Lazy<GUIStyle>(() => {
        var style = new GUIStyle(GUI.skin.label);
        style.alignment = TextAnchor.UpperCenter;
        return style;
    });

    public static Rect CenterRect(float centerX, float centerY, float width, float height,
            float maxWidth = -1, float maxHeight = -1) {
        if (maxWidth != -1 && width > maxWidth) {
            width = maxWidth;
        }
        if (maxHeight != -1 && height > maxHeight) {
            height = maxHeight;
        }
        return new Rect(centerX - width / 2, centerY - height / 2, width, height);
    }

    public static Rect HorizCenterRect(float centerX, float y, float width, float height,
            float maxWidth = -1, float maxHeight = -1) {
        if (maxWidth != -1 && width > maxWidth) {
            width = maxWidth;
        }
        if (maxHeight != -1 && height > maxHeight) {
            height = maxHeight;
        }
        return new Rect(centerX - width / 2, y, width, height);
    }

    public static GUIContent PadContent(string text, Texture image) =>
        new GUIContent("  " + text, image);

    public static GUIContent MenuContent(string text, Texture image) =>
        new GUIContent("    " + text, image);

    public static bool HighlightedButton(string text, GUIStyle style = null, bool highlight = true, params GUILayoutOption[] options) {
        if (style == null) {
            style = GUI.skin.button;
        }
        if (highlight) {
            return !GUILayout.Toggle(true, text, style, options);
        } else {
            return GUILayout.Button(text, style, options);
        }
    }

    public static bool HighlightedButton(Texture image, GUIStyle style = null, bool highlight = true, params GUILayoutOption[] options) {
        if (style == null) {
            style = GUI.skin.button;
        }
        if (highlight) {
            return !GUILayout.Toggle(true, image, style, options);
        } else {
            return GUILayout.Button(image, style, options);
        }
    }

    public static bool HighlightedButton(GUIContent content, GUIStyle style = null, bool highlight = true, params GUILayoutOption[] options) {
        if (style == null) {
            style = GUI.skin.button;
        }
        if (highlight) {
            return !GUILayout.Toggle(true, content, style, options);
        } else {
            return GUILayout.Button(content, style, options);
        }
    }

    public static void BeginButtonHorizontal(string name, GUIStyle style = null) {
        if (style == null) {
            style = GUI.skin.button;
        }
        GUILayout.BeginHorizontal(style);
    }

    public static bool EndButtonHorizontal(string name) {
        GUILayout.EndHorizontal();
        return EndButtonGroup(name);
    }

    public static void BeginButtonVertical(string name, GUIStyle style = null) {
        if (style == null) {
            style = GUI.skin.button;
        }
        GUILayout.BeginVertical(style);
    }

    public static bool EndButtonVertical(string name) {
        GUILayout.EndVertical();
        return EndButtonGroup(name);
    }

    private static bool EndButtonGroup(string name) {
        Rect buttonRect = GUILayoutUtility.GetLastRect();
        return GUI.Button(buttonRect, "", GUIStyle.none);
    }

    // clipped areas have problems if they're empty, or if they're the only
    // widget in a group

    public static void BeginVerticalClipped(params GUILayoutOption[] options) {
        GUILayout.BeginScrollView(Vector2.zero, false, false,
            GUIStyle.none, GUIStyle.none, GUIStyle.none, options);
    }

    public static void EndVerticalClipped() {
        GUILayout.EndScrollView();
    }

    public static void BeginHorizontalClipped(params GUILayoutOption[] options) {
        BeginVerticalClipped(options);
        GUILayout.BeginHorizontal();
    }

    public static void EndHorizontalClipped() {
        GUILayout.EndHorizontal();
        GUILayout.EndScrollView();
    }

    public static void ShowDisabled() {
        if (!GUI.enabled) {
            GUI.color *= new Color(1, 1, 1, 0.5f); // aaaaaaaaa
        } else {
            GUI.enabled = false;
        }
    }
}
