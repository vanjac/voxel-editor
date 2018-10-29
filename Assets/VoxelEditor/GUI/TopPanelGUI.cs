using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TopPanelGUI : GUIPanel
{
    private GUIPanel prevTopPanel;

    public virtual void Start()
    {
        prevTopPanel = GUIPanel.topPanel;
        if (prevTopPanel != null)
            prevTopPanel.enabled = false;
        GUIPanel.topPanel = this;
    }

    public virtual void OnDestroy()
    {
        if (prevTopPanel != null)
            prevTopPanel.enabled = true;
        GUIPanel.topPanel = prevTopPanel;
    }

    public override Rect GetRect(float width, float height)
    {
        return new Rect(GUIPanel.leftPanel.panelRect.xMax, 0,
            width - GUIPanel.leftPanel.panelRect.xMax, 0);
    }
}