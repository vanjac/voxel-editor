﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GUIPanel : MonoBehaviour
{
    protected const float targetHeight = 1080;

    private static GUISkin globalGUISkin = null;
    public GUISkin guiSkin;

    private static List<GUIPanel> openPanels = new List<GUIPanel>();

    public Vector2 scroll = new Vector2(0, 0);

    protected Vector2 touchStartPos = Vector2.zero;
    protected bool horizontalSlide, verticalSlide;
    protected bool holdOpen = false;
    protected bool stealFocus = true;
    protected float scaleFactor;

    private Rect panelRect;

    public virtual void OnEnable()
    {
        openPanels.Add(this);
    }

    public virtual void OnDisable()
    {
        openPanels.Remove(this);
    }

    private bool IsFocused()
    {
        for (int i = openPanels.Count - 1; i >= 0; i-- )
        {
            if (openPanels[i] == this)
                return true;
            if (openPanels[i].stealFocus)
                return false;
        }
        return false;
    }

    public void OnGUI()
    {
        if (globalGUISkin == null)
            globalGUISkin = guiSkin;
        GUI.skin = globalGUISkin;

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

            if (touch.phase == TouchPhase.Began && !PanelContainsPoint(touch.position) && !holdOpen)
                Destroy(this);
        }
        else
        {
            horizontalSlide = false;
            verticalSlide = false;
        }

        if (Input.GetButtonDown("Cancel") && !holdOpen)
        {
            if (IsFocused())
                Destroy(this);
        }

        scaleFactor = Screen.height / targetHeight;
        GUI.matrix = Matrix4x4.Scale(new Vector3(scaleFactor, scaleFactor, 1));
        float scaledScreenWidth = Screen.width / scaleFactor;

        Rect newPanelRect = GetRect(scaledScreenWidth, targetHeight);
        if (newPanelRect.width == 0)
            newPanelRect.width = panelRect.width;
        if (newPanelRect.height == 0)
            newPanelRect.height = panelRect.height;
        panelRect = newPanelRect;
        panelRect = GUILayout.Window(GetHashCode(), panelRect, _WindowGUI, "", GUILayout.ExpandHeight(true));
    }

    private void _WindowGUI(int id)
    {
        if (IsFocused())
            GUI.color = Color.white;
        else
            GUI.color = new Color(1, 1, 1, 0.4f);

        if (verticalSlide && Input.touchCount == 1 && IsFocused())
        {
            GUI.enabled = false;
            GUI.color = new Color(1, 1, 1, 2); // reverse disabled tinting
            if (Event.current.type == EventType.Repaint) // scroll at correct rate
                scroll.y += Input.GetTouch(0).deltaPosition.y / scaleFactor;
        }

        if (GetName() != "")
        {
            GUIStyle centered = new GUIStyle(GUI.skin.label);
            centered.alignment = TextAnchor.UpperCenter;
            GUILayout.Label(GetName(), centered);
        }

        WindowGUI();

        GUI.enabled = true;
        GUI.color = Color.white;
    }

    public abstract Rect GetRect(float width, float height);

    public virtual string GetName()
    {
        return "";
    }

    public abstract void WindowGUI();

    public bool PanelContainsPoint(Vector2 point)
    {
        point.y = Screen.height - point.y;
        point /= scaleFactor;
        return panelRect.Contains(point);
    }

    public static GUIPanel PanelContainingPoint(Vector2 point)
    {
        for (int i = openPanels.Count - 1; i >= 0; i--)
            if (openPanels[i].PanelContainsPoint(point))
                return openPanels[i];
        return null;
    }
}
