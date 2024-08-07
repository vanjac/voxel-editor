﻿using UnityEngine;

public class DialogGUI : GUIPanel {
    public string message;
    public string yesButtonText;
    public string noButtonText;
    public System.Action yesButtonHandler;
    public System.Action noButtonHandler;

    private bool calledHandler = false;

    public static DialogGUI ShowMessageDialog(GameObject gameObject, string message) {
        DialogGUI dialog = gameObject.AddComponent<DialogGUI>();
        dialog.message = message;
        dialog.yesButtonText = StringSet.Ok;
        return dialog;
    }

    public override Rect GetRect(Rect safeRect, Rect screenRect) =>
        GUIUtils.HorizCenterRect(safeRect.center.x, safeRect.center.y - 162, 576, 0);

    public override void WindowGUI() {
        GUILayout.Label(message, GUIUtils.LABEL_WORD_WRAPPED.Value);
        GUILayout.Space(32);
        GUILayout.BeginHorizontal();
        if (yesButtonText != null &&
                GUILayout.Button(GUIUtils.PadContent(yesButtonText, IconSet.done), StyleSet.buttonSmall)) {
            if (yesButtonHandler != null) {
                yesButtonHandler();
            }
            calledHandler = true;
            Destroy(this);
        }
        if (noButtonText != null
                && GUILayout.Button(GUIUtils.PadContent(noButtonText, IconSet.x), StyleSet.buttonSmall)) {
            if (noButtonHandler != null) {
                noButtonHandler();
            }
            calledHandler = true;
            Destroy(this);
        }
        GUILayout.EndHorizontal();
    }

    public void OnDestroy() {
        if (!calledHandler) {
            if (noButtonText != null) {
                if (noButtonHandler != null) {
                    noButtonHandler();
                }
            } else if (yesButtonText != null) {
                if (yesButtonHandler != null) {
                    yesButtonHandler();
                }
            }
        }
    }
}


public class TextInputDialogGUI : GUIPanel {
    public System.Action<string> handler;
    // TODO not called when touch keyboard not supported
    public System.Action cancelHandler;
    public string prompt;
    public string text = "";

    private TouchScreenKeyboard keyboard;
    private bool touchKeyboardSupported;

    public override Rect GetRect(Rect safeRect, Rect screenRect) {
        if (touchKeyboardSupported) {
            return new Rect(0, 0, 0, 0);
        } else {
            return GUIUtils.CenterRect(safeRect.center.x, safeRect.center.y, 576, 324);
        }
    }

    public override GUIStyle GetStyle() {
        if (touchKeyboardSupported) {
            return GUIStyle.none;
        } else {
            return GUI.skin.box;
        }
    }

    public override void OnEnable() {
        holdOpen = true;
        touchKeyboardSupported = TouchScreenKeyboard.isSupported;
        base.OnEnable();
    }

    void Start() {
        if (touchKeyboardSupported) {
            keyboard = TouchScreenKeyboard.Open(text, TouchScreenKeyboardType.ASCIICapable,
                false, false, false, false, // autocorrect, multiline, password, alert mode
                prompt);
            keyboard.selection = new RangeInt(0, text.Length);
        } else {
            title = prompt;
        }
    }

    public override void WindowGUI() {
        if (touchKeyboardSupported) {
            if (keyboard == null) {
                Destroy(this);
            } else if (keyboard.status == TouchScreenKeyboard.Status.Done) {
                handler(keyboard.text);
                keyboard = null; // WindowGUI could get called again
                Destroy(this);
            } else if (keyboard.status != TouchScreenKeyboard.Status.Visible) {
                if (cancelHandler != null) {
                    cancelHandler();
                }
                Destroy(this);
            }
        } else {
            text = GUILayout.TextField(text);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button(StringSet.Done)) {
                handler(text);
                Destroy(this);
            }
        }
    }
}


public class LargeMessageGUI : GUIPanel {
    public string message;
    public System.Action closeHandler;

    private static readonly System.Lazy<GUIStyle> dialogStyle = new System.Lazy<GUIStyle>(() => {
        var style = new GUIStyle(GUI.skin.window);
        style.normal.background = GUI.skin.button.normal.background; // more opaque :/
        return style;
    });

    public static LargeMessageGUI ShowLargeMessageDialog(GameObject gameObject, string message) {
        var dialog = gameObject.AddComponent<LargeMessageGUI>();
        dialog.message = message;
        return dialog;
    }

    public override Rect GetRect(Rect safeRect, Rect screenRect) =>
        GUIUtils.CenterRect(safeRect.center.x, safeRect.center.y,
            safeRect.width * .6f, safeRect.height * .6f, maxWidth: 1280, maxHeight: 800);

    public override GUIStyle GetStyle() => dialogStyle.Value;

    void OnDestroy() {
        if (closeHandler != null) {
            closeHandler();
        }
    }

    public override void WindowGUI() {
        scroll = GUILayout.BeginScrollView(scroll);
        GUILayout.Label(message, GUIUtils.LABEL_WORD_WRAPPED.Value);
        GUILayout.FlexibleSpace();
        GUILayout.EndScrollView();
        if (GUILayout.Button(StringSet.Ok)) {
            Destroy(this);
        }
    }
}