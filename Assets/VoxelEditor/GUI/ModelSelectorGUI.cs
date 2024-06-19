using System.Linq;
using UnityEngine;

public class ModelSelectorGUI : GUIPanel
{
    public System.Action<string> handler;

    private int selectedCategory = 0;
    private Texture2D[] categoryIcons;
    private Texture2D[] modelThumbnails;

    public override Rect GetRect(Rect safeRect, Rect screenRect) =>
        GUIUtils.CenterRect(safeRect.center.x, safeRect.center.y, 960, safeRect.height * .8f,
            maxHeight: 1360);

    void Start()
    {
        categoryIcons = ResourcesDirectory.GetModelDatabase().categories.Select(cat => cat.icon)
            .ToArray();
    }

    private ModelCategory GetCategory() =>
        ResourcesDirectory.GetModelDatabase().categories[selectedCategory];

    public override void WindowGUI()
    {
        int tab = GUILayout.SelectionGrid(selectedCategory, categoryIcons, categoryIcons.Length,
            StyleSet.buttonTab);
        if (tab != selectedCategory || modelThumbnails == null)
        {
            selectedCategory = tab;
            scroll = Vector2.zero;
            scrollVelocity = Vector2.zero;

            modelThumbnails = GetCategory().models.Select(
                name => ResourcesDirectory.GetModelThumbnail(name)).ToArray();
        }

        scroll = GUILayout.BeginScrollView(scroll);
        int selection = GUILayout.SelectionGrid(-1, modelThumbnails, 4, StyleSet.buttonSmall);
        if (selection != -1)
        {
            handler(GetCategory().models[selection]);
            Destroy(this);
        }
        GUILayout.EndScrollView();
    }
}
