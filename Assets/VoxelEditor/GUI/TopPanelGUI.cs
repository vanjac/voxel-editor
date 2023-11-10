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

    public override Rect GetRect(Rect safeRect, Rect screenRect)
    {
        return new Rect(GUIPanel.leftPanel.panelRect.xMax, safeRect.yMin,
            safeRect.xMax - GUIPanel.leftPanel.panelRect.xMax, 0);
    }
}