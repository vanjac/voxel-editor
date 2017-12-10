using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GUIPanel : MonoBehaviour {

    public const float targetHeight = 1080;

    static GUISkin globalGUISkin = null;
    public GUISkin guiSkin;

    static List<GUIPanel> openPanels = new List<GUIPanel>();
    static int frontDepth = 0;

    public Rect panelRect;
    public Vector2 scroll = new Vector2(0, 0);
    public float scaleFactor;
    public float scaledScreenWidth;
    public int depth = 0;

    protected Vector2 touchStartPos = Vector2.zero;
    protected bool horizontalSlide, verticalSlide;
    protected bool holdOpen = false;

    public virtual void OnEnable()
    {
        openPanels.Add(this);
        if (depth < frontDepth)
            frontDepth = depth;
    }

    public virtual void OnDisable()
    {
        openPanels.Remove(this);
        frontDepth = 999;
        foreach (GUIPanel panel in openPanels)
        {
            if (panel.depth < frontDepth)
                frontDepth = panel.depth;
        }
    }

    public virtual void OnGUI()
    {
        if (globalGUISkin == null)
            globalGUISkin = guiSkin;

        GUI.skin = globalGUISkin;
        GUI.depth = 1;
        GUI.enabled = true;
        if (depth > frontDepth)
            GUI.color = new Color(1, 1, 1, 0.4f);
        else
            GUI.color = Color.white;
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
                touchStartPos = touch.position;
            if ((!verticalSlide) && Mathf.Abs((touch.position - touchStartPos).x) > Screen.height * .06)
            {
                horizontalSlide = true;
            }
            if ((!horizontalSlide) && Mathf.Abs((touch.position - touchStartPos).y) > Screen.height * .06)
            {
                verticalSlide = true;
            }

            if (verticalSlide && PanelContainsPoint(touch.position))
            {
                GUI.enabled = false;
                GUI.color = new Color(1, 1, 1, 2); // reverse disabled tinting
                if (Event.current.type == EventType.Repaint) // scroll at correct rate
                    scroll.y += touch.deltaPosition.y / scaleFactor;
            }
            if (touch.phase == TouchPhase.Began && !PanelContainsPoint(touch.position)
                    && depth < 0 && !holdOpen)
                Destroy(this);
        }
        else
        {
            horizontalSlide = false;
            verticalSlide = false;
        }

        scaleFactor = Screen.height / targetHeight;
        GUI.matrix = Matrix4x4.Scale(new Vector3(scaleFactor, scaleFactor, 1));
        scaledScreenWidth = Screen.width / scaleFactor;
    }

    public bool PanelContainsPoint(Vector2 point)
    {
        point.y = Screen.height - point.y;
        point /= scaleFactor;
        return panelRect.Contains(point);
    }

    public static GUIPanel PanelContainingPoint(Vector2 point)
    {
        if (openPanels.Count == 0)
            return null;

        GUIPanel match = null;
        foreach (GUIPanel panel in openPanels)
        {
            if (panel.PanelContainsPoint(point))
            {
                if (match == null || panel.depth < match.depth)
                    match = panel;
            }
        }
        return match;
    }
}
