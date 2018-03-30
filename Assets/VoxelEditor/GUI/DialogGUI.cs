using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DialogGUI : GUIPanel
{
    public delegate void ButtonHandler();

    public string message;
    public string yesButtonText;
    public string noButtonText;
    public ButtonHandler yesButtonHandler;
    public ButtonHandler noButtonHandler;

    private GUIStyle messageLabelStyle;

    public override Rect GetRect(float width, float height)
    {
        return new Rect(width * .35f, height * .35f, width * .3f, height * .3f);
    }

    public override void WindowGUI()
    {
        if (messageLabelStyle == null)
        {
            messageLabelStyle = new GUIStyle(GUI.skin.label);
            messageLabelStyle.wordWrap = true;
        }

        GUILayout.Label(message, messageLabelStyle);
        GUILayout.FlexibleSpace();
        GUILayout.BeginHorizontal();
        if (yesButtonText != null && GUILayout.Button(yesButtonText))
        {
            if (yesButtonHandler != null)
                yesButtonHandler();
            Destroy(this);
        }
        if (noButtonText != null && GUILayout.Button(noButtonText))
        {
            if (noButtonHandler != null)
                noButtonHandler();
            Destroy(this);
        }
        GUILayout.EndHorizontal();
    }
}


public class TextInputDialogGUI : GUIPanel
{
    public delegate void TextHandler(string text);

    public TextHandler handler;
    public string prompt;

    private TouchScreenKeyboard keyboard;

    public override Rect GetRect(float width, float height)
    {
        return new Rect(0, 0, 0, 0);
    }

    public override GUIStyle GetStyle()
    {
        return GUIStyle.none;
    }

    public override void OnEnable()
    {
        holdOpen = true;
        base.OnEnable();
    }

    void Start()
    {
        keyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.ASCIICapable,
            false, false, false, false, // autocorrect, multiline, password, alert mode
            prompt);
    }

    public override void WindowGUI()
    {
        if (keyboard == null)
            Destroy(this);
        else if (keyboard.status == TouchScreenKeyboard.Status.Done)
        {
            handler(keyboard.text);
            Destroy(this);
        }
        else if (keyboard.status != TouchScreenKeyboard.Status.Visible)
            Destroy(this);
    }
}