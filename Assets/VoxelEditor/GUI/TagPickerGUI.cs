using UnityEngine;

public class TagPickerGUI : GUIPanel
{
    public System.Action<byte> handler;
    public bool multiple;
    public byte multiSelection;

    public override Rect GetRect(Rect safeRect, Rect screenRect) =>
        new Rect(GUIPanel.leftPanel.panelRect.xMax,
            GUIPanel.topPanel.panelRect.yMax, 960, 540);

    void Start()
    {
        // must be in Start bc multiple is not known on enable
        showCloseButton = multiple;
    }

    void OnDestroy()
    {
        if (multiple)
            handler(multiSelection);
    }

    public override void WindowGUI()
    {
        GUILayout.BeginHorizontal();
        for (byte i = 0; i < 4; i++)
            TagButton(i);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        for (byte i = 4; i < 8; i++)
            TagButton(i);
        GUILayout.EndHorizontal();
    }

    private void TagButton(byte tag)
    {
        byte bit = (byte)(1 << tag);
        if (!GUIUtils.HighlightedButton(IconSet.tagIcons[tag],
                highlight: (multiSelection & bit) != 0, options: GUILayout.ExpandHeight(true)))
            return;
        if (multiple)
        {
            multiSelection ^= bit;
        }
        else
        {
            handler(tag);
            Destroy(this);
        }
    }
}