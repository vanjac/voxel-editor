using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MaterialSelectorGUI : GUIPanel
{
    private const int NUM_COLUMNS = 4;
    private const int NUM_COLUMNS_ROOT = 6;
    private const int TEXTURE_MARGIN = 20;
    private const float CATEGORY_BUTTON_HEIGHT = 110;

    private enum Page { MATERIALS, COLOR }
    private Page page = Page.MATERIALS;

    public System.Action<VoxelFaceLayer> handler;
    public PaintLayer layer = PaintLayer.BASE;
    public bool allowNullMaterial = false;
    public VoxelFaceLayer selected;
    private CustomMaterial selectedCustom = null; // selected.material will also be set
    public VoxelArrayEditor voxelArray;

    private bool importFromWorld = false;
    private WorldFileReader selectedWorld = null;
    private string worldName = "";
    private string selectedCategory = ""; // empty string for root
    private string importMessage = null;

    // Objects listed in Materials page:
    private List<Material> materials; // built-in
    private List<Texture2D> thumbnails;
    private List<CustomMaterial> customMaterials;
    private string[] categories;
    private string[] worlds;

    private ColorPickerGUI colorPicker;
    // created an instance of the selected material?

    private static readonly System.Lazy<GUIStyle> categoryButtonStyle = new System.Lazy<GUIStyle>(() =>
    {
        var style = new GUIStyle(GUIStyleSet.instance.buttonLarge);
        style.padding.left = 0;
        style.padding.right = 0;
        return style;
    });

    public static readonly System.Lazy<GUIStyle> categoryLabelStyle = new System.Lazy<GUIStyle>(() =>
    {
        var style = new GUIStyle(GUIStyleSet.instance.labelTitle);
        style.alignment = TextAnchor.MiddleCenter;
        style.fixedHeight = GUIStyleSet.instance.buttonLarge.fixedHeight;
        return style;
    });

    public override void OnEnable()
    {
        showCloseButton = true;
        base.OnEnable();
    }

    public void Start()
    {
        OpenCurrentWorld();
        CategorySelected("");

        if (selected.material != null && CustomMaterial.IsCustomMaterial(selected.material))
        {
            foreach (var mat in voxelArray.customMaterials[(int)layer])
            {
                if (mat.material == selected.material)
                {
                    selectedCustom = mat;
                    break;
                }
            }
        }
    }

    void OnDestroy()
    {
        AssetManager.UnusedAssets();
    }

    public override Rect GetRect(Rect safeRect, Rect screenRect)
    {
        return GUIUtils.CenterRect(safeRect.center.x, safeRect.center.y,
            safeRect.width * .5f, safeRect.height * .8f, maxWidth: 1280);
    }

    public override void WindowGUI()
    {
        GUILayout.BeginHorizontal();
        TutorialGUI.TutorialHighlight("material type");
        TutorialGUI.ClearHighlight();
        GUILayout.EndHorizontal();

        if (page == Page.COLOR)
        {
            if (!ColorPage() && colorPicker != null)
            {
                Destroy(colorPicker);
                colorPicker = null;
            }
        }
        else if (colorPicker != null)
        {
            Destroy(colorPicker);
            colorPicker = null;
        }
        if (page == Page.MATERIALS)
            MaterialsPage();
        else
            scrollVelocity = Vector2.zero;
    }

    private bool ColorPage()
    {
        GUILayout.BeginHorizontal();
        if (ActionBarGUI.ActionBarButton(GUIIconSet.instance.close))
            page = Page.MATERIALS;
        GUILayout.Label("Adjust color", categoryLabelStyle.Value);
        GUILayout.EndHorizontal();

        if (colorPicker == null)
        {
            colorPicker = gameObject.AddComponent<ColorPickerGUI>();
            colorPicker.enabled = false;
            colorPicker.SetColor(selected.color);
            colorPicker.includeAlpha = layer == PaintLayer.OVERLAY;
            colorPicker.handler = (Color c) =>
            {
                selected.color = c;
                if (handler != null)
                    handler(selected);
            };
        }
        colorPicker.WindowGUI();
        return true;
    }

    private void MaterialsPage()
    {
        if (materials == null)
            return;

        GUILayout.BeginHorizontal();

        bool wasEnabled = GUI.enabled;
        Color baseColor = GUI.color;
        if (selectedCategory == "" && !importFromWorld)
        {
            if (!GUI.enabled)
                GUI.color *= new Color(1, 1, 1, 0.5f); // aaaaaaaaa
            else
                GUI.enabled = false;
        }
        if (ActionBarGUI.ActionBarButton(GUIIconSet.instance.close))
            BackButton();
        GUI.enabled = wasEnabled;
        GUI.color = baseColor;

        if ((layer == PaintLayer.BASE || layer == PaintLayer.OVERLAY) && !importFromWorld)
        {
            if (ActionBarGUI.ActionBarButton(GUIIconSet.instance.imageImport))
                ImportFromPhotos();
            if (ActionBarGUI.ActionBarButton(GUIIconSet.instance.worldImport))
                OpenWorldList();
            if (selected.material != null && CustomMaterial.IsSupportedShader(selected.material)
                    && ActionBarGUI.ActionBarButton(GUIIconSet.instance.copy))
                CloneCustomMaterial(selected.material, CustomDestinationCategory());
            if (selectedCustom != null)
            {
                if (ActionBarGUI.ActionBarButton(GUIIconSet.instance.draw))
                    EditCustomMaterial(selectedCustom);
                if (ActionBarGUI.ActionBarButton(GUIIconSet.instance.delete))
                {
                    var dialog = gameObject.AddComponent<DialogGUI>();
                    dialog.message = "Are you sure you want to delete this custom material?";
                    dialog.yesButtonText = "Yes";
                    dialog.noButtonText = "No";
                    dialog.yesButtonHandler = () => DeleteCustomMaterial();
                }
            }
        }
        // prevent from expanding window
        GUIUtils.BeginHorizontalClipped(GUILayout.ExpandHeight(false));
        string labelText = "";
        if (importFromWorld)
        {
            if (selectedWorld == null)
                labelText = "Import from world...";
            else if (selectedCategory == "")
                labelText = worldName + " (import)";
            else
                labelText = worldName + " / " + selectedCategory + " (import)";
        }
        else
            labelText = selectedCategory;
        GUILayout.Label(labelText, categoryLabelStyle.Value);
        GUIUtils.EndHorizontalClipped();

        Color baseBGColor = GUI.backgroundColor;
        GUI.backgroundColor *= selected.color;
        if (ActionBarGUI.ActionBarButton(GUIIconSet.instance.color))
            page = Page.COLOR;
        GUI.backgroundColor = baseBGColor;

        GUILayout.EndHorizontal();

        scroll = GUILayout.BeginScrollView(scroll);

        if (importFromWorld && selectedWorld == null)
        {
            int selected = GUILayout.SelectionGrid(-1, worlds, 1, categoryButtonStyle.Value);
            if (selected != -1)
            {
                WorldSelected(worlds[selected]);
                CategorySelected("");
            }
            GUILayout.EndScrollView();
            return;
        }

        if (importMessage != null)
            GUILayout.Label(importMessage);
        else if (materials.Count == 0 && customMaterials.Count == 0 && categories.Length == 0)
            GUILayout.Label("World contains no materials for "
                + (layer == PaintLayer.OVERLAY ? "overlay." : "base."));

        Rect rowRect = new Rect();
        int numColumns = selectedCategory == "" ? NUM_COLUMNS_ROOT : NUM_COLUMNS;
        int buttonI = 0;
        if (allowNullMaterial && selectedCategory == "" && !importFromWorld)
        {
            if (MaterialButton(null, numColumns, buttonI++, ref rowRect, GUIIconSet.instance.noLarge))
                MaterialSelected(null);
        }
        foreach (var mat in customMaterials)
        {
            if (MaterialButton(mat.material, numColumns, buttonI++, ref rowRect, null, "Custom"))
            {
                if (importFromWorld)
                {
                    // import
                    OpenCurrentWorld();
                    CloneCustomMaterial(mat.material, mat.category);
                }
                else
                    CustomMaterialSelected(mat);
            }
        }
        for (int i = 0; i < materials.Count; i++)
        {
            if (MaterialButton(materials[i], numColumns, buttonI++, ref rowRect, thumbnails[i]))
                MaterialSelected(materials[i]);
        }

        if (selectedCategory == "" && categories.Length > 0)
        {
            int selected = GUILayout.SelectionGrid(-1, categories, NUM_COLUMNS,
                categoryButtonStyle.Value);
            if (selected != -1)
                CategorySelected(categories[selected]);
        }

        GUILayout.EndScrollView();
    }

    private bool MaterialButton(Material material, int numColumns, int i, ref Rect rowRect,
                               Texture thumbnail=null, string label=null)
    {
        if (i % numColumns == 0)
            rowRect = GUILayoutUtility.GetAspectRect(numColumns);
        Rect buttonRect = rowRect;
        buttonRect.width = buttonRect.height;
        buttonRect.x = buttonRect.width * (i % numColumns);
        Rect textureRect = new Rect(
            buttonRect.xMin + TEXTURE_MARGIN, buttonRect.yMin + TEXTURE_MARGIN,
            buttonRect.width - TEXTURE_MARGIN * 2, buttonRect.height - TEXTURE_MARGIN * 2);
        bool pressed;
        if (material == selected.material)
            // highlight the button
            pressed = !GUI.Toggle(buttonRect, true, "", GUI.skin.button);
        else
            pressed = GUI.Button(buttonRect, "");
        if (thumbnail != null)
            GUI.DrawTexture(textureRect, thumbnail);
        else
            DrawFaceLayer(new VoxelFaceLayer {material = material, color = Color.white}, textureRect);
        if (label != null)
        {
            Rect labelRect = new Rect(textureRect.min,
                GUIStyleSet.instance.buttonSmall.CalcSize(new GUIContent(label)));
            GUI.Label(labelRect, label, GUIStyleSet.instance.buttonSmall);
        }
        return pressed;
    }

    private void BackButton()
    {
        if (importFromWorld && selectedWorld == null)
        {
            OpenCurrentWorld();
            CategorySelected("");
        }
        else if (selectedCategory == "" && selectedWorld != null)
            OpenWorldList();
        else
            CategorySelected("");
    }

    private void CategorySelected(string category)
    {
        selectedCategory = category;
        importMessage = null;
        scroll = Vector2.zero;
        scrollVelocity = Vector2.zero;

        materials = new List<Material>();
        thumbnails = new List<Texture2D>();
        if (importFromWorld)
        {
            try
            {
                customMaterials = selectedWorld.FindCustomMaterials(layer, category);
            }
            catch (MapReadException e)
            {
                importMessage = e.Message;
                Debug.LogError(e.InnerException);
            }
        }
        else
        {
            foreach (MaterialInfo info in ResourcesDirectory.materialInfos.Values)
            {
                if (info.layer != layer || info.category != category)
                    continue;
                materials.Add(ResourcesDirectory.LoadMaterial(info, true));
                thumbnails.Add(info.thumbnail);
            }

            customMaterials = new List<CustomMaterial>();
            if ((int)layer < voxelArray.customMaterials.Length)
            {
                foreach (CustomMaterial mat in voxelArray.customMaterials[(int)layer])
                {
                    if (mat.layer == layer && mat.category == category)
                        customMaterials.Add(mat);
                }
            }
        }

        AssetManager.UnusedAssets();
    }

    // doesn't reset selected category, and doesn't load materials!
    private void WorldSelected(string name)
    {
        worldName = name;
        importFromWorld = true;
        importMessage = null;

        materials = new List<Material>();
        thumbnails = new List<Texture2D>();
        customMaterials = new List<CustomMaterial>();

        try
        {
            selectedWorld = ReadWorldFile.ReadPath(WorldFiles.GetNewWorldPath(name));
            categories = selectedWorld.GetCustomMaterialCategories(layer).ToArray();
        }
        catch (MapReadException e)
        {
            importMessage = e.Message;
            Debug.LogError(e.InnerException);
            scroll = Vector2.zero;
            scrollVelocity = Vector2.zero;
        }

        AssetManager.UnusedAssets();
    }

    private void OpenCurrentWorld()
    {
        importFromWorld = false;
        selectedWorld = null;
        importMessage = null;

        materials = new List<Material>();
        thumbnails = new List<Texture2D>();
        customMaterials = new List<CustomMaterial>();

        var categoriesSet = new SortedSet<string>();
        foreach (MaterialInfo info in ResourcesDirectory.materialInfos.Values)
        {
            if (info.layer == layer && info.category != "")
                categoriesSet.Add(info.category);
        }
        if ((int)layer < voxelArray.customMaterials.Length)
        {
            foreach (CustomMaterial mat in voxelArray.customMaterials[(int)layer])
            {
                if (mat.category != "")
                    categoriesSet.Add(mat.category);
            }
        }
        categories = new string[categoriesSet.Count];
        categoriesSet.CopyTo(categories);

        AssetManager.UnusedAssets();
    }

    private void OpenWorldList()
    {
        importFromWorld = true;
        selectedWorld = null;
        selectedCategory = "";
        scroll = Vector2.zero;
        scrollVelocity = Vector2.zero;

        materials = new List<Material>(); // free up assets
        thumbnails = new List<Texture2D>();
        var worldNames = new List<string>();
        WorldFiles.ListWorlds(new List<string>(), worldNames);
        worlds = worldNames.ToArray();

        AssetManager.UnusedAssets();
    }

    private void MaterialSelected(Material material)
    {
        selected.material = material;
        selected.color = Color.white; // reset to default
        selectedCustom = null;
        if (handler != null)
            handler(selected);
    }

    private void CustomMaterialSelected(CustomMaterial mat)
    {
        MaterialSelected(mat.material);
        selectedCustom = mat;
    }

    private void ImportFromPhotos()
    {
        NativeGalleryWrapper.ImportTexture((Texture2D texture) => {
            if (texture == null)
                return;
            CustomMaterial customMat = new CustomMaterial(layer);
            customMat.texture = texture;
            customMat.category = CustomDestinationCategory();
            AddCustomMaterial(customMat);
            CustomMaterialSelected(customMat);
            EditCustomMaterial(customMat);
        });
    }

    private void AddCustomMaterial(CustomMaterial customMat)
    {
        voxelArray.customMaterials[(int)layer].Insert(0, customMat); // add to top
        voxelArray.unsavedChanges = true;
    }

    private string CustomDestinationCategory()
    {
        return selectedCategory == "" ? CustomMaterial.DEFAULT_CATEGORY : selectedCategory;
    }

    private void EditCustomMaterial(CustomMaterial customMat)
    {
        PropertiesGUI propsGUI = GetComponent<PropertiesGUI>();
        if (propsGUI != null)
        {
            propsGUI.specialSelection = customMat;
            propsGUI.normallyOpen = true;
            Destroy(this);
        }
    }

    private void CloneCustomMaterial(Material material, string category)
    {
        CustomMaterial newTex = new CustomMaterial(Material.Instantiate(material), layer);
        newTex.category = category;
        AddCustomMaterial(newTex);
        CategorySelected(newTex.category);
        CustomMaterialSelected(newTex);
        voxelArray.unsavedChanges = true;
    }

    private void DeleteCustomMaterial()
    {
        // TODO different shaders
        Material replacement = ResourcesDirectory.FindMaterial(
                layer == PaintLayer.OVERLAY ? "MATTE_overlay" : "MATTE", true);
        voxelArray.ReplaceMaterial(selected.material, replacement);
        if (!voxelArray.customMaterials[(int)layer].Remove(selectedCustom))
            Debug.LogError("Error removing material");
        MaterialSelected(replacement);
        voxelArray.unsavedChanges = true;
    }

    public static void DrawFaceLayer(VoxelFaceLayer faceLayer, Rect rect)
    {
        if (faceLayer.material == null)
            return;
        
        Color baseColor = GUI.color;
        // fix transparent colors becoming opaque while scrolling
        if (GUI.color.a > 1)
            GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, 1);

        Color matColor = faceLayer.color;
        if (matColor.a == 0.0f)
            matColor = new Color(matColor.r, matColor.g, matColor.b, 0.6f);
        GUI.color *= matColor;

        Texture texture = Texture2D.whiteTexture;
        Vector2 textureScale = Vector2.one;
        if (faceLayer.material.HasProperty("_MainTex") && faceLayer.material.mainTexture != null)
        {
            texture = faceLayer.material.mainTexture;
            textureScale = faceLayer.material.mainTextureScale;
        }
        else if (faceLayer.material.HasProperty("_FrontTex"))  // 6-sided skybox
        {
            texture = faceLayer.material.GetTexture("_FrontTex");
            GUI.color *= 2;
        }

        GUI.DrawTextureWithTexCoords(rect, texture, new Rect(Vector2.zero, textureScale));
        GUI.color = baseColor;
    }
}
