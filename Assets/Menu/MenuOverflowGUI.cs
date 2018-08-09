using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuOverflowGUI : GUIPanel
{
    public TextAsset creditsText;

    public override Rect GetRect(float width, float height)
    {
        return new Rect(width * .8f, 0, width * .2f, 0);
    }

    public override GUIStyle GetStyle()
    {
        return GUIStyle.none;
    }

    public override void OnEnable()
    {
        holdOpen = true;
        stealFocus = false;
        base.OnEnable();
    }

    public override void WindowGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        if (ActionBarGUI.ActionBarButton(GUIIconSet.instance.overflow))
        {
            var overflow = gameObject.AddComponent<OverflowMenuGUI>();
            overflow.items = new OverflowMenuGUI.MenuItem[]
            {
                new OverflowMenuGUI.MenuItem("Help", GUIIconSet.instance.help, () => {
                    gameObject.AddComponent<HelpGUI>();
                }),
                new OverflowMenuGUI.MenuItem("About", GUIIconSet.instance.about, () =>
                {
                    LargeMessageGUI.ShowLargeMessageDialog(gameObject, creditsText.text);
                })
            };
        }
        GUILayout.EndHorizontal();
    }
}