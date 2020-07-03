using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemplatePickerGUI : GUIPanel
{
    private GUIContent[] options;
    public System.Action<int> handler;

    private static readonly System.Lazy<GUIStyle> buttonStyle = new System.Lazy<GUIStyle>(() =>
    {
        var style = new GUIStyle(GUIStyleSet.instance.buttonSmall);
        style.imagePosition = ImagePosition.ImageAbove;
        return style;
    });

    public override Rect GetRect(Rect safeRect, Rect screenRect)
    {
        return GUIUtils.HorizCenterRect(safeRect.center.x, 180,
            900, 480);
    }

    void Start() {
        title = "New World";
        options = new GUIContent[]
        {
            new GUIContent("Indoor", GUIIconSet.instance.indoorLarge),
            new GUIContent("Floating", GUIIconSet.instance.floatingLarge)
        };
    }

    public override void WindowGUI()
    {
        int selection = GUILayout.SelectionGrid(-1, options, 2, buttonStyle.Value, GUILayout.ExpandHeight(true));
        if (selection != -1)
        {
            handler(selection);
            Destroy(this);
        }
    }
}
