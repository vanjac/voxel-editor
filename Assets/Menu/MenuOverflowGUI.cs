using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuOverflowGUI : GUIPanel
{
    public TextAsset creditsText;

    public override Rect GetRect(Rect safeRect, Rect screenRect)
    {
        return new Rect(safeRect.xMin + safeRect.width * .8f, safeRect.yMin,
            safeRect.width * .2f, 0);
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
                        + "\n\n----------\n\nSystem Info:\nResolution: {3}x{4}\nDPI: {5}"
                        + "\nAudio: {6}Hz {7}",
                        Application.version, Application.unityVersion, creditsText.text,
                        Screen.width, Screen.height, Screen.dpi,
                        AudioSettings.outputSampleRate, AudioSettings.speakerMode);
                    LargeMessageGUI.ShowLargeMessageDialog(gameObject, text);
                }),
                new OverflowMenuGUI.MenuItem("Website", GUIIconSet.instance.website, () =>
                {
                    Application.OpenURL("https://chroma.zone/voxel-editor/");
                }),
                new OverflowMenuGUI.MenuItem("Subreddit", GUIIconSet.instance.reddit, () =>
                {
                    Application.OpenURL("https://www.reddit.com/r/nspace/");
                }),
                new OverflowMenuGUI.MenuItem("Videos", GUIIconSet.instance.youTube, () =>
                {
                    Application.OpenURL("https://www.youtube.com/playlist?list=PLMiQPjIk5IrpgNcQY5EUYaGFDuAf7PLY2");
                }),
                new OverflowMenuGUI.MenuItem("Donate", GUIIconSet.instance.donate, () =>
                {
                    Application.OpenURL("https://ko-fi.com/chroma_zone");
                })
            };
        }
        GUILayout.EndHorizontal();
    }
}