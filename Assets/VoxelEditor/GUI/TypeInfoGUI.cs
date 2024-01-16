using UnityEngine;

public class TypeInfoGUI : GUIPanel
{
    public PropertiesObjectType type;

    public override Rect GetRect(Rect safeRect, Rect screenRect) =>
        new Rect(GUIPanel.leftPanel.panelRect.xMax,
            GUIPanel.topPanel.panelRect.yMax, 960, 0);

    public override void WindowGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.Label(type.icon, GUILayout.ExpandWidth(false));
        GUILayout.BeginVertical();
        GUILayout.BeginHorizontal();
        GUILayout.Label(type.displayName(StringSet), StyleSet.labelTitle);
        if (GUILayout.Button(StringSet.Done, GUILayout.ExpandWidth(false)))
            Destroy(this);
        GUILayout.EndHorizontal();
        GUILayout.Label("<i>" + type.description(StringSet) + "</i>",
            GUIUtils.LABEL_WORD_WRAPPED.Value);
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();
        var longDesc = type.longDescription(StringSet);
        if (longDesc != "")
        {
            GUILayout.BeginVertical(GUI.skin.box);
            GUILayout.Label(longDesc, GUIUtils.LABEL_WORD_WRAPPED.Value);
            GUILayout.EndVertical();
        }
    }
}