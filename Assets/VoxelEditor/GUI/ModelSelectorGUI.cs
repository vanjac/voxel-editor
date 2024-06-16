using System.Linq;
using UnityEngine;

public class ModelSelectorGUI : GUIPanel
{
    public System.Action<string> handler;

    private int selectedCategory = 0;
    private string[] categoryNames;
    private string[] modelNames;

    public override Rect GetRect(Rect safeRect, Rect screenRect) =>
        GUIUtils.CenterRect(safeRect.center.x, safeRect.center.y, 960, safeRect.height * .8f,
            maxHeight: 1360);

    void Start()
    {
        categoryNames = ResourcesDirectory.GetModelDatabase().categories.Select(cat => cat.name)
            .ToArray();
    }

    public override void WindowGUI()
    {
        int tab = GUILayout.SelectionGrid(selectedCategory, categoryNames, categoryNames.Count(),
            StyleSet.buttonTab);
        if (tab != selectedCategory || modelNames == null)
        {
            selectedCategory = tab;
            scroll = Vector2.zero;
            scrollVelocity = Vector2.zero;

            var category = ResourcesDirectory.GetModelDatabase().categories[selectedCategory];
            modelNames = category.models.ToArray();
        }

        scroll = GUILayout.BeginScrollView(scroll);
        int selection = GUILayout.SelectionGrid(-1, modelNames, 1, StyleSet.buttonLarge);
        if (selection != -1)
        {
            handler(modelNames[selection]);
            Destroy(this);
        }
        GUILayout.EndScrollView();
    }
}
