using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIManager : MonoBehaviour
{
    // minimum supported targetHeight value
    // (so the largest supported interface scale)
    private const int MIN_TARGET_HEIGHT = 1080;
    // the maximum height of a screen that would still be considered a phone
    // (and not a "phablet" or tablet). this is the maximum screen height that
    // would still use MIN_TARGET_HEIGHT -- anything bigger will use higher
    // values for targetHeight.
    // 2.7" is the height of a 5.5" diagonal screen with 16:9 ratio.
    private const float MAX_PHONE_HEIGHT_INCHES = 2.7f;

    public static GUIManager instance;

    public GUISkin guiSkin;
    public float targetHeightOverride = 0; // for testing

    public GUIIconSet iconSet;
    private GUIStyleSet _styleSet;
    public GUIStyleSet styleSet
    {
        get
        {
            if (_styleSet == null)
                _styleSet = new GUIStyleSet(GUI.skin);
            return _styleSet;
        }
    }
    // fix bug that causes fonts to be unloaded when Resources.UnloadUnusedAssets is called
    public Font[] alternateFonts;

    void Start()
    {
        instance = this;
        GUIPanel.guiSkin = guiSkin;
        if (Screen.dpi <= 0)
        {
            Debug.Log("Unknown screen DPI!");
        }
        Update();
    }

    void Update()
    {
        float scaledScreenHeight;
        if (targetHeightOverride != 0)
            scaledScreenHeight = targetHeightOverride;
        else if (Screen.dpi <= 0)
            scaledScreenHeight = MIN_TARGET_HEIGHT;
        else
        {
            float screenHeightInches = Screen.height / Screen.dpi;
            if (screenHeightInches < MAX_PHONE_HEIGHT_INCHES)
                scaledScreenHeight = MIN_TARGET_HEIGHT;
            else
                scaledScreenHeight = (MIN_TARGET_HEIGHT / MAX_PHONE_HEIGHT_INCHES) * screenHeightInches;
        }
        GUIPanel.scaleFactor = Screen.height / scaledScreenHeight;
        GUIPanel.scaledScreenArea = new Rect(0, 0,
            Screen.width / GUIPanel.scaleFactor,
            Screen.height / GUIPanel.scaleFactor);
        var safeArea = Screen.safeArea;
        GUIPanel.scaledSafeArea = new Rect(
            safeArea.xMin / GUIPanel.scaleFactor,
            (Screen.height - safeArea.yMax) / GUIPanel.scaleFactor, // y axis is reversed for GUI
            safeArea.width / GUIPanel.scaleFactor,
            safeArea.height / GUIPanel.scaleFactor);
        GUIPanel.guiMatrix = Matrix4x4.Scale(new Vector3(GUIPanel.scaleFactor, GUIPanel.scaleFactor, 1));
    }
}
