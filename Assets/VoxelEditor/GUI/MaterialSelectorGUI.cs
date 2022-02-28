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
    private const string WORLD_LIST = "Import from world...";

    public delegate void MaterialSelectHandler(Material material);

    public MaterialSelectHandler handler;
    public PaintLayer layer = PaintLayer.BASE;
    public bool allowNullMaterial = false;
    public Material highlightMaterial = null; // the current selected material
    private CustomTexture highlightCustom = null; // highlightMaterial will also be set
    public VoxelArrayEditor voxelArray;

    private int tab = 0;
    private string selectedWorld = null; // null for current world, WORLD_LIST for list
    private string selectedCategory = ""; // empty string for root
    private bool loadingWorld;
    private string importMessage = null;

    // Objects listed in Textures tab:
    private List<Material> materials; // built-in
    private List<Texture2D> thumbnails;
    private string[] categories;
    private string[] worlds;
    private List<CustomTexture> worldCustomTextures; // not filtered by category

    private ColorPickerGUI colorPicker;
    // created an instance of the selected material?
    private bool instance;

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
        instance = false;

        if (highlightMaterial != null && CustomTexture.IsCustomTexture(highlightMaterial))
        {
            foreach (var tex in voxelArray.customTextures[(int)layer])
            {
                if (tex.material == highlightMaterial)
                {
                    highlightCustom = tex;
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

        if (tab == 1)
        {
            if (!ColorTab() && colorPicker != null)
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
        if (tab == 0)
            TextureTab();
        else
        {
            scrollVelocity = Vector2.zero;
        }
    }

    private bool ColorTab()
    {
        GUILayout.BeginHorizontal();
        if (ActionBarGUI.ActionBarButton(GUIIconSet.instance.close))
            tab = 0;
        GUILayout.Label("Adjust color", categoryLabelStyle.Value);
        GUILayout.EndHorizontal();

        if (colorPicker == null)
        {
            colorPicker = gameObject.AddComponent<ColorPickerGUI>();
            colorPicker.enabled = false;
            Color currentColor = highlightMaterial.color;
            colorPicker.SetColor(currentColor);
            colorPicker.includeAlpha = layer == PaintLayer.OVERLAY;
            colorPicker.handler = (Color c) =>
            {
                MakeInstance();
                highlightMaterial.color = c;
                if (handler != null)
                    handler(highlightMaterial);
            };
        }
        colorPicker.WindowGUI();
        return true;
    }

    private void TextureTab()
    {
        if (materials == null)
            return;
        if (loadingWorld)
        {
            GUILayout.Label("Loading world...");
            return;
        }

        GUILayout.BeginHorizontal();

        bool wasEnabled = GUI.enabled;
        Color baseColor = GUI.color;
        if (selectedCategory == "" && selectedWorld == null)
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

        if ((layer == PaintLayer.BASE || layer == PaintLayer.OVERLAY) && selectedWorld == null)
        {
            if (ActionBarGUI.ActionBarButton(GUIIconSet.instance.newTexture))
                ImportTextureFromPhotos();
            if (ActionBarGUI.ActionBarButton(GUIIconSet.instance.worldImport))
                OpenWorldList();
            if (highlightMaterial != null && CustomTexture.IsSupportedShader(highlightMaterial)
                    && ActionBarGUI.ActionBarButton(GUIIconSet.instance.copy))
                CloneCustomTexture(highlightMaterial, CustomDestinationCategory());
            if (highlightCustom != null)
            {
                if (ActionBarGUI.ActionBarButton(GUIIconSet.instance.draw))
                    EditCustomTexture(highlightCustom);
                if (ActionBarGUI.ActionBarButton(GUIIconSet.instance.delete))
                {
                    var dialog = gameObject.AddComponent<DialogGUI>();
                    dialog.message = "Are you sure you want to delete this custom texture?";
                    dialog.yesButtonText = "Yes";
                    dialog.noButtonText = "No";
                    dialog.yesButtonHandler = () => DeleteCustomTexture();
                }
            }
        }
        // prevent from expanding window
        GUIUtils.BeginHorizontalClipped(GUILayout.ExpandHeight(false));
        string labelText = "";
        if (selectedWorld != null)
        {
            labelText = selectedWorld;
            if (selectedCategory != "")
                labelText += " / " + selectedCategory;
            if (selectedWorld != WORLD_LIST)
                labelText += " (import)";
        }
        else
            labelText = selectedCategory;
        GUILayout.Label(labelText, categoryLabelStyle.Value);
        GUIUtils.EndHorizontalClipped();
        if (selectedWorld == null && highlightMaterial != null
            && !CustomTexture.IsCustomTexture(highlightMaterial))
        {
            Color baseBGColor = GUI.backgroundColor;
            GUI.backgroundColor *= highlightMaterial.color;
            if (ActionBarGUI.ActionBarButton(GUIIconSet.instance.color))
                tab = 1;
            GUI.backgroundColor = baseBGColor;
        }
        GUILayout.EndHorizontal();

        scroll = GUILayout.BeginScrollView(scroll);

        if (selectedWorld == WORLD_LIST)
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

        if (selectedWorld != null && importMessage != null)
            GUILayout.Label(importMessage);

        Rect rowRect = new Rect();
        int numColumns = selectedCategory == "" ? NUM_COLUMNS_ROOT : NUM_COLUMNS;
        int textureI = 0;
        if (allowNullMaterial && selectedCategory == "" && selectedWorld == null)
        {
            if (TextureButton(null, numColumns, textureI++, ref rowRect, GUIIconSet.instance.noLarge))
                MaterialSelected(null);
        }
        foreach (var tex in worldCustomTextures)
        {
            if (tex.category != selectedCategory)
                continue;
            if (TextureButton(tex.material, numColumns, textureI++, ref rowRect, null, "Custom"))
            {
                if (selectedWorld != null)
                {
                    // import
                    OpenCurrentWorld();
                    CloneCustomTexture(tex.material, tex.category);
                }
                else
                    CustomTextureSelected(tex);
            }
        }
        for (int i = 0; i < materials.Count; i++)
        {
            if (TextureButton(materials[i], numColumns, textureI++, ref rowRect, thumbnails[i]))
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

    private bool TextureButton(Material material, int numColumns, int i, ref Rect rowRect,
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
        bool selected;
        if (material == highlightMaterial || (material != null && highlightMaterial != null
                && material.name == highlightMaterial.name))
            // highlight the button
            selected = !GUI.Toggle(buttonRect, true, "", GUI.skin.button);
        else
            selected = GUI.Button(buttonRect, "");
        if (thumbnail != null)
            GUI.DrawTexture(textureRect, thumbnail);
        else
            DrawMaterialTexture(material, textureRect);
        if (label != null)
        {
            Rect labelRect = new Rect(textureRect.min,
                GUIStyleSet.instance.buttonSmall.CalcSize(new GUIContent(label)));
            GUI.Label(labelRect, label, GUIStyleSet.instance.buttonSmall);
        }
        return selected;
    }

    private void MakeInstance()
    {
        if (!instance)
        {
            //Debug.Log("instantiate");
            highlightMaterial = ResourcesDirectory.InstantiateMaterial(highlightMaterial);
            highlightCustom = null;
            instance = true;
        }
    }

    private void BackButton()
    {
        if (selectedWorld == WORLD_LIST)
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
        scroll = Vector2.zero;
        scrollVelocity = Vector2.zero;

        if (selectedWorld == null)
        {
            materials = new List<Material>();
            thumbnails = new List<Texture2D>();

            foreach (MaterialInfo info in ResourcesDirectory.materialInfos.Values)
            {
                if (info.layer != layer || info.category != category)
                    continue;
                materials.Add(ResourcesDirectory.LoadMaterial(info, true));
                thumbnails.Add(info.thumbnail);
            }
        }

        AssetManager.UnusedAssets();
    }

    // doesn't reset selected category!
    private void WorldSelected(string world)
    {
        selectedWorld = world;

        materials = new List<Material>();
        thumbnails = new List<Texture2D>();
        worldCustomTextures = new List<CustomTexture>();
        categories = new string[0];
        StartCoroutine(LoadWorldCoroutine(WorldFiles.GetNewWorldPath(selectedWorld)));

        AssetManager.UnusedAssets();
    }

    private void OpenCurrentWorld()
    {
        selectedWorld = null;
        worldCustomTextures = voxelArray.customTextures[(int)layer];

        var categoriesSet = GetCustomTextureCategories();
        foreach (MaterialInfo info in ResourcesDirectory.materialInfos.Values)
        {
            if (info.layer == layer && info.category != "")
                categoriesSet.Add(info.category);
        }
        categories = new string[categoriesSet.Count];
        categoriesSet.CopyTo(categories);
    }

    private void OpenWorldList()
    {
        selectedWorld = WORLD_LIST;
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

    private SortedSet<string> GetCustomTextureCategories()
    {
        var categoriesSet = new SortedSet<string>();
        foreach (CustomTexture tex in worldCustomTextures)
        {
            if (tex.category != "")
                categoriesSet.Add(tex.category);
        }
        return categoriesSet;
    }

    private void MaterialSelected(Material material)
    {
        highlightMaterial = material;
        highlightCustom = null;
        instance = false;
        if (handler != null)
            handler(highlightMaterial);
    }

    private void CustomTextureSelected(CustomTexture tex)
    {
        MaterialSelected(tex.material);
        highlightCustom = tex;
    }

    private void ImportTextureFromPhotos()
    {
        NativeGalleryWrapper.ImportTexture((Texture2D texture) => {
            if (texture == null)
                return;
            CustomTexture customTex = new CustomTexture(layer);
            customTex.texture = texture;
            customTex.category = CustomDestinationCategory();
            AddCustomTexture(customTex);
            CustomTextureSelected(customTex);
            EditCustomTexture(customTex);
        });
    }

    private void AddCustomTexture(CustomTexture customTex)
    {
        voxelArray.customTextures[(int)layer].Insert(0, customTex); // add to top
        voxelArray.unsavedChanges = true;
    }

    private string CustomDestinationCategory()
    {
        return selectedCategory == "" ? CustomTexture.DEFAULT_CATEGORY : selectedCategory;
    }

    private void EditCustomTexture(CustomTexture customTex)
    {
        PropertiesGUI propsGUI = GetComponent<PropertiesGUI>();
        if (propsGUI != null)
        {
            propsGUI.specialSelection = customTex;
            propsGUI.normallyOpen = true;
            Destroy(this);
        }
    }

    private void CloneCustomTexture(Material material, string category)
    {
        CustomTexture newTex = new CustomTexture(Material.Instantiate(material), layer);
        newTex.category = category;
        AddCustomTexture(newTex);
        CategorySelected(newTex.category);
        CustomTextureSelected(newTex);
        voxelArray.unsavedChanges = true;
    }

    private void DeleteCustomTexture()
    {
        // TODO different shaders
        Material replacement = ResourcesDirectory.InstantiateMaterial(ResourcesDirectory.FindMaterial(
                layer == PaintLayer.OVERLAY ? "MATTE_overlay" : "MATTE", true));
        replacement.color = highlightCustom.color;
        voxelArray.ReplaceMaterial(highlightMaterial, replacement);
        if (!voxelArray.customTextures[(int)layer].Remove(highlightCustom))
            Debug.LogError("Error removing material");
        MaterialSelected(replacement);
        instance = true;
        voxelArray.unsavedChanges = true;
    }

    private IEnumerator LoadWorldCoroutine(string path)
    {
        // copied from DataImportGUI
        loadingWorld = true;
        importMessage = null;
        yield return null;
        yield return null;
        try
        {
            worldCustomTextures = ReadWorldFile.ReadCustomTextures(path, layer);
            if (worldCustomTextures.Count == 0)
                importMessage = "World contains no custom textures for "
                    + (layer == PaintLayer.OVERLAY ? "overlay." : "base.");
            var categoriesSet = GetCustomTextureCategories();
            categories = new string[categoriesSet.Count];
            categoriesSet.CopyTo(categories);
        }
        catch (MapReadException e)
        {
            importMessage = e.Message;
            Debug.LogError(e.InnerException);
        }
        finally
        {
            loadingWorld = false;
        }
    }

    public static void DrawMaterialTexture(Material mat, Rect rect)
    {
        if (mat == null)
            return;
        
        Color baseColor = GUI.color;
        // fix transparent colors becoming opaque while scrolling
        if (GUI.color.a > 1)
            GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, 1);

        Color matColor = mat.color;
        if (matColor.a == 0.0f)
            matColor = new Color(matColor.r, matColor.g, matColor.b, 0.6f);
        GUI.color *= matColor;

        Texture texture = Texture2D.whiteTexture;
        Vector2 textureScale = Vector2.one;
        if (mat.HasProperty("_MainTex") && mat.mainTexture != null)
        {
            texture = mat.mainTexture;
            textureScale = mat.mainTextureScale;
        }
        else if (mat.HasProperty("_FrontTex"))  // 6-sided skybox
        {
            texture = mat.GetTexture("_FrontTex");
            GUI.color *= 2;
        }

        GUI.DrawTextureWithTexCoords(rect, texture, new Rect(Vector2.zero, textureScale));
        GUI.color = baseColor;
    }
}
