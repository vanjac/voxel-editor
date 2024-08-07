﻿using UnityEngine;

public class TypePickerGUI : GUIPanel {
    public System.Action<PropertiesObjectType> handler;
    public PropertiesObjectType[][] categories;
    public string[] categoryNames = new string[0];

    private int selectedCategory;
    private PropertiesObjectType showHelp;

    private static readonly System.Lazy<GUIStyle> descriptionStyle = new System.Lazy<GUIStyle>(() => {
        var style = new GUIStyle(GUI.skin.label);
        style.wordWrap = true;
        style.padding = new RectOffset(0, 0, 0, 0);
        style.margin = new RectOffset(0, 0, 0, 0);
        return style;
    });

    private static readonly System.Lazy<GUIStyle> helpIconStyle = new System.Lazy<GUIStyle>(() => {
        var style = new GUIStyle(GUI.skin.label);
        style.padding = new RectOffset(0, 0, 0, 0);
        //style.margin = new RectOffset(0, 0, 0, 0);
        return style;
    });

    public override Rect GetRect(Rect safeRect, Rect screenRect) =>
        GUIUtils.CenterRect(safeRect.center.x, safeRect.center.y, 960, safeRect.height * .8f,
            maxHeight: 1360);

    public override void WindowGUI() {
        if (categoryNames.Length > 1) {
            int tab = GUILayout.SelectionGrid(selectedCategory, categoryNames,
                categoryNames.Length, StyleSet.buttonTab);
            if (tab != selectedCategory) {
                selectedCategory = tab;
                scroll = Vector2.zero;
                scrollVelocity = Vector2.zero;
            }
        }

        var categoryItems = categories[selectedCategory];
        scroll = GUILayout.BeginScrollView(scroll);
        for (int i = 0; i < categoryItems.Length; i++) {
            PropertiesObjectType item = categoryItems[i];
            GUIUtils.BeginButtonVertical(item.fullName);
            GUILayout.BeginHorizontal();
            GUILayout.Label(item.icon, GUILayout.ExpandWidth(false));
            GUILayout.BeginVertical();
            GUILayout.BeginHorizontal();
            GUILayout.Label(item.displayName(StringSet), StyleSet.labelTitle);
            var longDesc = item.longDescription(StringSet);
            if (longDesc != "" && GUILayout.Button(IconSet.helpCircle,
                    helpIconStyle.Value, GUILayout.ExpandWidth(false))) {
                if (showHelp == item) {
                    showHelp = null;
                } else {
                    showHelp = item;
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Label("<i>" + item.description(StringSet) + "</i>", descriptionStyle.Value);
            GUILayout.EndVertical();
            GUILayout.EndHorizontal();
            if (showHelp == item) {
                GUILayout.Space(16);
                GUILayout.Label(longDesc, descriptionStyle.Value);
            }
            if (GUIUtils.EndButtonVertical(item.fullName)) {
                handler(item);
                Destroy(this);
            }
        }
        GUILayout.EndScrollView();
    }
}