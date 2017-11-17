using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIPanel : MonoBehaviour {

    public const float targetHeight = 360;

    static GUISkin globalGUISkin = null;
    public GUISkin guiSkin;

    static List<GUIPanel> openPanels = new List<GUIPanel>();

    public Rect panelRect;
    public Vector2 scroll = new Vector2(0, 0);
    public float scaleFactor;
    public float scaledScreenWidth;
    public int depth = 0;

    public void OnEnable()
    {
        openPanels.Add(this);
    }

    public void OnDisable()
    {
        openPanels.Remove(this);
    }

    public void OnGUI()
    {
        if (globalGUISkin == null)
            globalGUISkin = guiSkin;

        GUI.skin = guiSkin;
        GUI.depth = 1;

        scaleFactor = Screen.height / targetHeight;
        GUI.matrix = Matrix4x4.Scale(new Vector3(scaleFactor, scaleFactor, 1));
        scaledScreenWidth = Screen.width / scaleFactor;
    }

    public static GUIPanel PanelContainingPoint(Vector2 point)
    {
        if (openPanels.Count == 0)
            return null;

        point.y = Screen.height - point.y;
        point /= openPanels[0].scaleFactor;

        GUIPanel match = null;
        foreach (GUIPanel panel in openPanels)
        {
            if (panel.panelRect.Contains(point))
            {
                if (match == null || panel.depth < match.depth)
                    match = panel;
            }
        }
        return match;
    }
}
