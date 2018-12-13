using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuOverflowGUI : GUIPanel
{
    public TextAsset creditsText;

    public override Rect GetRect(Rect maxRect)
    {
        return new Rect(maxRect.xMin + maxRect.width * .8f, maxRect.yMin,
            maxRect.width * .2f, 0);
    }

    public override GUIStyle GetStyle()
    {
        return GUIStyle.none;
    }

    void Start()
    {
        GUIPanel.topPanel = this;
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
                    string text = System.String.Format("Version {0}\nMade with Unity {1}\n\n{2}"
                        + "\n\n----------\n\nSystem Info:\nResolution: {3}x{4}\nDPI: {5}",
                        Application.version, Application.unityVersion, creditsText.text,
                        Screen.width, Screen.height, Screen.dpi);
                    LargeMessageGUI.ShowLargeMessageDialog(gameObject, text);
                })
            };
        }
        GUILayout.EndHorizontal();
    }
}