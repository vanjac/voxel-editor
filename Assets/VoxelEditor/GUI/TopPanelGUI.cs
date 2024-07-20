using UnityEngine;

public abstract class TopPanelGUI : GUIPanel {
    private GUIPanel prevTopPanel;

    public override void OnEnable() {
        holdOpen = true;
        stealFocus = false;

        base.OnEnable();
    }

    public virtual void Start() {
        prevTopPanel = GUIPanel.topPanel;
        if (prevTopPanel != null) {
            prevTopPanel.enabled = false;
        }
        GUIPanel.topPanel = this;
    }

    public virtual void OnDestroy() {
        if (prevTopPanel != null) {
            prevTopPanel.enabled = true;
            prevTopPanel.PushToBack();
        }
        GUIPanel.topPanel = prevTopPanel;
    }

    public override Rect GetRect(Rect safeRect, Rect screenRect) =>
        new Rect(GUIPanel.leftPanel.panelRect.xMax, safeRect.yMin,
            safeRect.xMax - GUIPanel.leftPanel.panelRect.xMax, 0);
}