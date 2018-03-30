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