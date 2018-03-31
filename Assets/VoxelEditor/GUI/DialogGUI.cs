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

    public static void ShowMessageDialog(GameObject gameObject, string message)
    {
        DialogGUI dialog = gameObject.AddComponent<DialogGUI>();
        dialog.message = message;
        dialog.yesButtonText = "OK";
    }

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
    private bool touchKeyboardSupported;
    private string text = "";

    public override Rect GetRect(float width, float height)
    {
        if (touchKeyboardSupported)
            return new Rect(0, 0, 0, 0);
        else
            return new Rect(width * .35f, height * .35f, width * .3f, height * .3f);
    }

    public override GUIStyle GetStyle()
    {
        if (touchKeyboardSupported)
            return GUIStyle.none;
        else
            return GUI.skin.box;
    }

    public override void OnEnable()
    {
        holdOpen = true;
        touchKeyboardSupported = TouchScreenKeyboard.isSupported;
        base.OnEnable();
    }

    void Start()
    {
        if (touchKeyboardSupported)
            keyboard = TouchScreenKeyboard.Open("", TouchScreenKeyboardType.ASCIICapable,
                false, false, false, false, // autocorrect, multiline, password, alert mode
                prompt);
    }

    public override void WindowGUI()
    {
        if (touchKeyboardSupported)
        {
            if (keyboard == null)
                Destroy(this);
            else if (keyboard.status == TouchScreenKeyboard.Status.Done)
            {
                handler(keyboard.text);
                keyboard = null; // WindowGUI could get called again
                Destroy(this);
            }
            else if (keyboard.status != TouchScreenKeyboard.Status.Visible)
                Destroy(this);
        }
        else
        {
            text = GUILayout.TextField(text);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Done"))
            {
                handler(text);
                Destroy(this);
            }
        }
    }
}