using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TypePickerGUI : GUIPanel
{
    public delegate void TypeHandler(System.Type type);

    public TypeHandler handler;
    public GameScripts.NamedType[] items;

    public override void OnEnable()
    {
        depth = -1;
        base.OnEnable();
    }

    public override void OnGUI()
    {
        base.OnGUI();

        panelRect = new Rect(scaledScreenWidth * .25f, targetHeight * .25f,
            scaledScreenWidth * .5f, targetHeight * .5f);
        GUILayout.BeginArea(panelRect, GUI.skin.box);

        for (int i = 0; i < items.Length; i++ )
            if (GUILayout.Button(items[i].name))
            {
                handler(items[i].type);
                Destroy(this);
            }

        GUILayout.EndArea();
    }
}