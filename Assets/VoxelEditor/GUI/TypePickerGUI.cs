using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TypePickerGUI : GUIPanel
{
    public delegate void TypeHandler(PropertiesObjectType type);

    public TypeHandler handler;
    public PropertiesObjectType[][] categories;
    public string[] categoryNames = new string[0];

    private int selectedCategory;

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

    public override Rect GetRect(Rect safeRect, Rect screenRect)
    {
        return GUIUtils.CenterRect(safeRect.center.x, safeRect.center.y, 960, safeRect.height * .8f);
    }

    public override void WindowGUI()
    {
        if (categoryNames.Length > 1)
        {
            int tab = GUILayout.SelectionGrid(selectedCategory, categoryNames,
                categoryNames.Length, GUI.skin.GetStyle("button_tab"));
            if (tab != selectedCategory)
            {
                selectedCategory = tab;
                scroll = Vector2.zero;
                scrollVelocity = Vector2.zero;
            }
        }

        var categoryItems = categories[selectedCategory];
        scroll = GUILayout.BeginScrollView(scroll);
        for (int i = 0; i < categoryItems.Length; i++)
        {
            PropertiesObjectType item = categoryItems[i];
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