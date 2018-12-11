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

    public GUISkin guiSkin;
    public float targetHeightOverride = 0; // for testing

    void Start()
    {
        GUIPanel.guiSkin = guiSkin;
        if (Screen.dpi <= 0)
        {
            Debug.Log("Unknown screen DPI!");
        }
        Update();
    }

    void Update()
    {
        if (targetHeightOverride != 0)
            GUIPanel.scaledScreenHeight = targetHeightOverride;
        else if (Screen.dpi <= 0)
            GUIPanel.scaledScreenHeight = MIN_TARGET_HEIGHT;
        else
        {
            float screenHeightInches = Screen.height / Screen.dpi;
            if (screenHeightInches < MAX_PHONE_HEIGHT_INCHES)
                GUIPanel.scaledScreenHeight = MIN_TARGET_HEIGHT;
            else
                GUIPanel.scaledScreenHeight = (MIN_TARGET_HEIGHT / MAX_PHONE_HEIGHT_INCHES) * screenHeightInches;
        }
        GUIPanel.scaleFactor = Screen.height / GUIPanel.scaledScreenHeight;
        GUIPanel.scaledScreenWidth = Screen.width / GUIPanel.scaleFactor;
        GUIPanel.guiMatrix = Matrix4x4.Scale(new Vector3(GUIPanel.scaleFactor, GUIPanel.scaleFactor, 1));
    }
}
