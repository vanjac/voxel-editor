using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TagPickerGUI : GUIPanel
{
    public delegate void TagHandler(byte tag);
    public TagHandler handler;

    private string[] tags;
    private static readonly System.Lazy<GUIStyle> buttonStyle = new System.Lazy<GUIStyle>(() =>
    {
        var style = new GUIStyle(GUI.skin.button);
        style.fontSize = GUI.skin.font.fontSize * 2;
        return style;
    });

    void Start()
    {
        tags = new string[Entity.NUM_TAGS];
        for (byte i = 0; i < Entity.NUM_TAGS; i++)
            tags[i] = Entity.TagToString(i);
    }

    public override Rect GetRect(Rect safeRect, Rect screenRect)
    {
        return new Rect(GUIPanel.leftPanel.panelRect.xMax,
            GUIPanel.topPanel.panelRect.yMax, 960, 540);
    }

    public override void WindowGUI()
    {
        int selection = GUILayout.SelectionGrid(-1, tags, 4, buttonStyle.Value, GUILayout.ExpandHeight(true));
        if (selection != -1)
        {
            handler((byte)selection);
            Destroy(this);
        }
    }
}