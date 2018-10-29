using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class GUIPanel : MonoBehaviour
{
    protected const float targetHeight = 1080;

    private static GUISkin globalGUISkin = null;
    public GUISkin guiSkin;

    private static List<GUIPanel> openPanels = new List<GUIPanel>();

    public static GameObject guiGameObject
    {
        get
        {
            // TODO: don't use GameObject.Find
            return GameObject.Find("GUI");
        }
    }

    public static GUIPanel leftPanel, topPanel;

    public string title = "";

    public Vector2 scroll = Vector2.zero;
    protected Vector2 scrollVelocity = Vector2.zero;

    protected Vector2 touchStartPos = Vector2.zero;
    protected bool panelSlide, horizontalSlide, verticalSlide;

    protected bool holdOpen = false;
    protected bool stealFocus = true;
    protected float scaleFactor;

    public Rect panelRect;

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
            {
                touchStartPos = touch.position;
                if (PanelContainsPoint(touch.position))
                {
                    scrollVelocity = Vector2.zero;
                    panelSlide = true;
                }
                else if (!holdOpen)
                {
                    GUIPanel touchedPanel = PanelContainingPoint(touch.position);
                    // if the panel is behind this one
                    if (openPanels.IndexOf(touchedPanel) < openPanels.IndexOf(this))
                        Destroy(this);
                }
            }
            if (panelSlide && !verticalSlide && Mathf.Abs((touch.position - touchStartPos).x) > Screen.height * .06)
            {
                horizontalSlide = true;
            }
            if (panelSlide && !horizontalSlide && Mathf.Abs((touch.position - touchStartPos).y) > Screen.height * .06)
            {
                verticalSlide = true;
            }
        }
        else
        {
            panelSlide = false;
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
        panelRect = GUILayout.Window(GetHashCode(), panelRect, _WindowGUI, "", GetStyle(), GUILayout.ExpandHeight(true));
    }

    private void _WindowGUI(int id)
    {
        if (!IsFocused())
        {
            GUI.enabled = false;
            GUI.color = new Color(1, 1, 1, 0.8f);
        } else if (verticalSlide && Input.touchCount == 1 && IsFocused())
        {
            Touch touch = Input.GetTouch(0);
            GUI.enabled = false;
            GUI.color = new Color(1, 1, 1, 2); // reverse disabled tinting
            float scrollVel = touch.deltaPosition.y / scaleFactor;
            if (Event.current.type == EventType.Repaint) // scroll at correct rate
                scroll.y += scrollVel;
            if (touch.phase == TouchPhase.Ended)
                scrollVelocity = new Vector2(0, scrollVel / touch.deltaTime);
            else
                scrollVelocity = Vector2.zero;
        }
        else
        {
            GUI.enabled = true;
            GUI.color = Color.white;
        }
        if (Event.current.type == EventType.Repaint)
        {
            scroll += scrollVelocity * Time.deltaTime;
            scrollVelocity *= .92f;
        }

        if (title != "")
        {
            GUILayout.Label(title, GUIUtils.LABEL_HORIZ_CENTERED.Value);
        }

        WindowGUI();

        GUI.enabled = true;
        GUI.color = Color.white;
    }

    public abstract Rect GetRect(float width, float height);

    public virtual GUIStyle GetStyle()
    {
        return GUI.skin.window;
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

    public void BringToFront()
    {
        GUI.BringWindowToFront(GetHashCode());
        openPanels.Remove(this);
        openPanels.Add(this);
    }

    protected void RotateAboutPoint(Vector2 point, float rotation, Vector2 scaleFactor)
    {
        Vector2 translation = point + panelRect.min;
        GUI.matrix *= Matrix4x4.Translate(translation);
        GUI.matrix *= Matrix4x4.Scale(new Vector3(scaleFactor.x, scaleFactor.y, 1));
        GUI.matrix *= Matrix4x4.Rotate(Quaternion.Euler(new Vector3(0, 0, rotation)));
        GUI.matrix *= Matrix4x4.Translate(-translation);
    }
}
