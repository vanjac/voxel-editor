using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIStyleSet
{
    public static GUIStyleSet instance
    {
        get
        {
            if (GUIManager.instance == null)
                return null;
            return GUIManager.instance.styleSet;
        }
    }

    public GUIStyle labelTitle, buttonTab, buttonLarge, buttonSmall;

    // must be created in OnGUI
    public GUIStyleSet(GUISkin skin)
    {
        labelTitle = skin.GetStyle("label_title");
        buttonTab = skin.GetStyle("button_tab");
        buttonLarge = skin.GetStyle("button_large");
        buttonSmall = skin.GetStyle("button_small");
    }
}
