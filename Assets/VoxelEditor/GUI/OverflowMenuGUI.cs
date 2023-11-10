using UnityEngine;

public class OverflowMenuGUI : GUIPanel
{
    public struct MenuItem
    {
        public string text;
        public Texture icon;
        public System.Action action;
        public bool stayOpen;

        public MenuItem(string text, Texture icon, System.Action action, bool stayOpen = false)
        {
            this.text = text;
            this.icon = icon;
            this.action = action;
            this.stayOpen = stayOpen;
        }
    }

    public MenuItem[] items;
    public int depth = 0;
    private int selected = -1;

    public static readonly System.Lazy<GUIStyle> buttonStyle = new System.Lazy<GUIStyle>(() =>
    {
        var style = new GUIStyle(GUIStyleSet.instance.buttonLarge);
        style.alignment = TextAnchor.MiddleLeft;
        return style;
    });

    public override Rect GetRect(Rect safeRect, Rect screenRect)
    {
        return new Rect(safeRect.xMax - 432 * (depth + 1),
            GUIPanel.topPanel.panelRect.yMax, 432, 0);
    }

    public override GUIStyle GetStyle()
    {
        return GUIStyle.none;
    }

    public override void OnEnable()
    {
        stealFocus = false;
        base.OnEnable();
    }

    public override void WindowGUI()
    {
        int i = 0;
        foreach (MenuItem item in items)
        {
            if (GUIUtils.HighlightedButton(GUIUtils.MenuContent(item.text, item.icon),
                buttonStyle.Value, i == selected))
            {
                item.action();
                if (!item.stayOpen)
                {
                    // destroy self and all parent menus
                    foreach (OverflowMenuGUI parentMenu in gameObject.GetComponents<OverflowMenuGUI>())
                        Destroy(parentMenu);
                }
                else
                {
                    selected = i;
                }
            }
            i++;
        }
    }
}
