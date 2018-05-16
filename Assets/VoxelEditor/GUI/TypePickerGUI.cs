using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TypePickerGUI : GUIPanel
{
    public delegate void TypeHandler(PropertiesObjectType type);

    public TypeHandler handler;
    public PropertiesObjectType[] items;

    private static readonly Lazy<GUIStyle> descriptionStyle = new Lazy<GUIStyle>(() =>
    {
        var style = new GUIStyle(GUI.skin.label);
        style.wordWrap = true;
        style.padding = new RectOffset(0, 0, 0, 0);
        style.margin = new RectOffset(0, 0, 0, 0);
        return style;
    });

    private static readonly Lazy<GUIStyle> helpIconStyle = new Lazy<GUIStyle>(() =>
    {
        var style = new GUIStyle(GUI.skin.label);
        style.padding = new RectOffset(0, 0, 0, 0);
        //style.margin = new RectOffset(0, 0, 0, 0);
        return style;
    });

    public override Rect GetRect(float width, float height)
    {
        return new Rect(width * .25f, height * .1f, width * .5f, height * .8f);
    }

    public override void WindowGUI()
    {
        scroll = GUILayout.BeginScrollView(scroll);
        for (int i = 0; i < items.Length; i++)
        {
            PropertiesObjectType item = items[i];
            GUIUtils.BeginButtonHorizontal(item.fullName);
            GUILayout.Label(item.icon, GUILayout.ExpandWidth(false));
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Label(item.fullName, GUI.skin.GetStyle("label_title"));
            if (item.longDescription != ""
                && GUILayout.Button(GUIIconSet.instance.helpCircle, helpIconStyle.Value, GUILayout.ExpandWidth(false)))
            {
                var typeInfo = gameObject.AddComponent<TypeInfoGUI>();
                typeInfo.type = item;
            }
            GUILayout.EndHorizontal();
            GUILayout.Label("<i>" + item.description + "</i>", descriptionStyle.Value);
            GUILayout.EndVertical();
            if (GUIUtils.EndButtonHorizontal(item.fullName))
            {
                handler(item);
                Destroy(this);
            }
        }
        GUILayout.EndScrollView();
    }
}