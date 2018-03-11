using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TagPickerGUI : GUIPanel
{
    public delegate void TagHandler(byte tag);
    public TagHandler handler;

    private string[] tags;
    private static GUIStyle buttonStyle = null;

    void Start()
    {
        tags = new string[Entity.NUM_TAGS];
        for (byte i = 0; i < Entity.NUM_TAGS; i++)
            tags[i] = Entity.TagToString(i);
    }

    public override Rect GetRect(float width, float height)
    {
        return new Rect(width * .25f, height * .25f, width * .5f, height * .5f);
    }

    public override void WindowGUI()
    {
        if (buttonStyle == null)
        {
            buttonStyle = new GUIStyle(GUI.skin.button);
            buttonStyle.fontSize = GUI.skin.font.fontSize * 2;
        }
        int selection = GUILayout.SelectionGrid(-1, tags, 4, buttonStyle, GUILayout.ExpandHeight(true));
        if (selection != -1)
        {
            handler((byte)selection);
            Destroy(this);
        }
    }
}