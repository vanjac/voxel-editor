using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TemplatePickerGUI : GUIPanel
{
    private GUIContent[] options;
    public System.Action<int> handler;

    private static readonly Lazy<GUIStyle> buttonStyle = new Lazy<GUIStyle>(() =>
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
            new GUIContent("Indoor", GUIIconSet.instance.indoor),
            new GUIContent("Floating", GUIIconSet.instance.floating)
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
