using System.Linq;
using UnityEngine;

public class ModelSelectorGUI : GUIPanel {
    public System.Action<string> handler;
    public string selectedModel = "";

    private int selectedCategory = 0;
    private int selectedIndex = -1;
    private Texture2D[] categoryIcons;
    private Texture2D[] modelThumbnails;

    public override Rect GetRect(Rect safeRect, Rect screenRect) =>
        GUIUtils.CenterRect(safeRect.center.x, safeRect.center.y, 960, safeRect.height * .8f,
            maxHeight: 1360);

    void Start() {
        var categories = ResourcesDirectory.GetModelCategories();
        categoryIcons = categories.Select(cat => cat.icon).ToArray();
        selectedCategory = categories.FindIndex(cat => cat.models.Contains(selectedModel));
        if (selectedCategory == -1) {
            selectedCategory = 0;
        }
        UpdateCategory();
    }

    private ResourcesDirectory.ModelCategory GetCategory() =>
        ResourcesDirectory.GetModelCategories()[selectedCategory];

    private void UpdateCategory() {
        scroll = Vector2.zero;
        scrollVelocity = Vector2.zero;

        var category = GetCategory();
        modelThumbnails = category.models.Select(
            name => ResourcesDirectory.GetModelThumbnail(name)).ToArray();
        selectedIndex = category.models.IndexOf(selectedModel);
    }

    public override void WindowGUI() {
        int tab = GUILayout.SelectionGrid(selectedCategory, categoryIcons, categoryIcons.Length,
            StyleSet.buttonTab);
        if (tab != selectedCategory) {
            selectedCategory = tab;
            UpdateCategory();
        }

        scroll = GUILayout.BeginScrollView(scroll);
        int selection = GUILayout.SelectionGrid(
            selectedIndex, modelThumbnails, 4, StyleSet.buttonSmall);
        if (selection != selectedIndex) {
            handler(GetCategory().models[selection]);
            Destroy(this);
        }
        GUILayout.EndScrollView();
    }
}
