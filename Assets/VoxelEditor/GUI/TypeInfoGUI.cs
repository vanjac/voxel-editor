using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TypeInfoGUI : GUIPanel
{
    public PropertiesObjectType type;

    public override Rect GetRect(float width, float height)
    {
        return new Rect(width * .25f, height * .25f, width * .5f, height * .5f);
    }

    public override void WindowGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(type.icon, GUILayout.ExpandWidth(false));
        GUILayout.BeginVertical();
        GUILayout.Label(type.fullName, GUI.skin.GetStyle("label_title"));
        GUILayout.Label(type.description, GUIUtils.LABEL_WORD_WRAPPED.Value);
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
        GUILayout.Label("<i>" + type.longDescription + "</i>", GUIUtils.LABEL_WORD_WRAPPED.Value);
    }
}