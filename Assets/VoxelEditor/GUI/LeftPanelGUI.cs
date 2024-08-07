﻿public abstract class LeftPanelGUI : GUIPanel {
    private GUIPanel prevLeftPanel;

    public override void OnEnable() {
        holdOpen = true;
        stealFocus = false;

        base.OnEnable();
    }

    public virtual void Start() {
        prevLeftPanel = GUIPanel.leftPanel;
        if (prevLeftPanel != null) {
            prevLeftPanel.enabled = false;
        }
        GUIPanel.leftPanel = this;
    }

    public virtual void OnDestroy() {
        if (prevLeftPanel != null) {
            prevLeftPanel.enabled = true;
            prevLeftPanel.PushToBack();
        }
        GUIPanel.leftPanel = prevLeftPanel;
    }
}