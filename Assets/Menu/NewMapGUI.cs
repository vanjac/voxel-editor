using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NewMapGUI : GUIPanel
{
    public delegate void NameHandler(string name);
    public NameHandler handler;
    private string mapName = "";

    public override Rect GetRect(float width, float height)
    {
        return new Rect(width * .35f, height * .35f, width * .3f, height * .3f);
    }

    public override void WindowGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        GUILayout.Label("Enter new map name...");
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        mapName = GUILayout.TextField(mapName);
        if (GUILayout.Button("Create"))
        {
            handler(mapName);
            Destroy(this);
        }
    }
}