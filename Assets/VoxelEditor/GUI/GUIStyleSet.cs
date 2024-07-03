using UnityEngine;

public class GUIStyleSet {
    public GUIStyle labelTitle, buttonTab, buttonLarge, buttonSmall;

    // must be created in OnGUI
    public GUIStyleSet(GUISkin skin) {
        labelTitle = skin.GetStyle("label_title");
        buttonTab = skin.GetStyle("button_tab");
        buttonLarge = skin.GetStyle("button_large");
        buttonSmall = skin.GetStyle("button_small");
    }
}
