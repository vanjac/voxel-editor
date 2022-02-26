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
    private const string CUSTOM_CATEGORY = " CUSTOM "; // leading space for sorting order (sorry)
    private const string WORLD_LIST_CATEGORY = "Import from world...";

    public delegate void MaterialSelectHandler(Material material);

    public MaterialSelectHandler handler;
    public PaintLayer layer = PaintLayer.MATERIAL;
    public bool allowNullMaterial = false;
    public Material highlightMaterial = null; // the current selected material
    private CustomTexture highlightCustom = null; // highlightMaterial will also be set
    public VoxelArrayEditor voxelArray;

    private int tab = 0;
    private string selectedCategory;
    private bool importFromWorld, loadingWorld;
    private string importMessage = null;
    private List<Material> materials; // built-in
    private List<Texture2D> thumbnails;
    private List<CustomTexture> customTextures;
    private string[] categories;
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
        if (selectedCategory == "")
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

        if (layer == PaintLayer.MATERIAL || layer == PaintLayer.OVERLAY)
        {
            if (ActionBarGUI.ActionBarButton(GUIIconSet.instance.newTexture))
                ImportTextureFromPhotos();
            if (ActionBarGUI.ActionBarButton(GUIIconSet.instance.worldImport))
                CategorySelected(WORLD_LIST_CATEGORY);
            if (highlightMaterial != null && ActionBarGUI.ActionBarButton(GUIIconSet.instance.copy))
                DuplicateCustomTexture(highlightMaterial);
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
        GUILayout.Label(selectedCategory, categoryLabelStyle.Value);
        GUIUtils.EndHorizontalClipped();
        if (highlightMaterial != null && !CustomTexture.IsCustomTexture(highlightMaterial))
        {
            Color baseBGColor = GUI.backgroundColor;
            GUI.backgroundColor *= highlightMaterial.color;
            if (ActionBarGUI.ActionBarButton(GUIIconSet.instance.paint))
                tab = 1;
            GUI.backgroundColor = baseBGColor;
        }
        GUILayout.EndHorizontal();

        scroll = GUILayout.BeginScrollView(scroll);

        if (importFromWorld && (importMessage != null))
            GUILayout.Label(importMessage);

        Rect rowRect = new Rect();
        int numColumns = categories.Length > 0 ? NUM_COLUMNS_ROOT : NUM_COLUMNS;
        int textureI = 0;
        if (allowNullMaterial && selectedCategory == "")
        {
            if (TextureButton(null, numColumns, textureI++, ref rowRect, GUIIconSet.instance.noLarge))
                MaterialSelected(null);
        }
        foreach (var tex in customTextures)
        {
            if (TextureButton(tex.material, numColumns, textureI++, ref rowRect))
            {
                if (importFromWorld)
                {
                    importFromWorld = false;
                    CategorySelected(CUSTOM_CATEGORY);
                    DuplicateCustomTexture(tex.material);
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

        if (categories.Length > 0)
        {
            int selectDir = GUILayout.SelectionGrid(-1, categories,
                selectedCategory == WORLD_LIST_CATEGORY ? 1 : NUM_COLUMNS,
                categoryButtonStyle.Value);
            if (selectDir != -1)
                CategorySelected(categories[selectDir]);
        }

        GUILayout.EndScrollView();
    }

    private bool TextureButton(Material material, int numColumns, int i, ref Rect rowRect,
                               Texture thumbnail=null)
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
        if (importFromWorld)
        {
            CategorySelected(WORLD_LIST_CATEGORY);
            importFromWorld = false;
        }
        else if (selectedCategory == WORLD_LIST_CATEGORY)
            CategorySelected(CUSTOM_CATEGORY);
        else
            CategorySelected("");
    }

    private void CategorySelected(string category)
    {
        if (selectedCategory == WORLD_LIST_CATEGORY && category != CUSTOM_CATEGORY)
            importFromWorld = true;
        selectedCategory = category;
        scroll = Vector2.zero;
        scrollVelocity = Vector2.zero;

        materials = new List<Material>();
        thumbnails = new List<Texture2D>();
        customTextures = new List<CustomTexture>();
        if (category == CUSTOM_CATEGORY)
        {
            categories = new string[0];
            foreach (var tex in voxelArray.customTextures[(int)layer])
                customTextures.Add(tex);
            AssetManager.UnusedAssets();
            return;
        }
        else if (category == WORLD_LIST_CATEGORY)
        {
            var worldNames = new List<string>();
            WorldFiles.ListWorlds(new List<string>(), worldNames);
            categories = worldNames.ToArray();
            AssetManager.UnusedAssets();
            return;
        }
        else if (importFromWorld)
        {
            categories = new string[0];
            // TODO :(
            StartCoroutine(LoadWorldCoroutine(WorldFiles.GetNewWorldPath(category)));
            return;
        }

        var categoriesSet = new SortedSet<string>();
        if (category == "" && (layer == PaintLayer.MATERIAL || layer == PaintLayer.OVERLAY))
            categoriesSet.Add(CUSTOM_CATEGORY);
        foreach (MaterialInfo info in ResourcesDirectory.materialInfos.Values)
        {
            if (info.layer != layer)
                continue;
            if (info.category != category)
            {
                if (category == "")
                    categoriesSet.Add(info.category);
                continue;
            }
            if (info.name.StartsWith("$"))
                continue; // special alternate materials for game
            materials.Add(ResourcesDirectory.LoadMaterial(info));
            thumbnails.Add(info.thumbnail);
        }
        categories = new string[categoriesSet.Count];
        categoriesSet.CopyTo(categories);

        AssetManager.UnusedAssets();
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

            voxelArray.customTextures[(int)layer].Add(customTex);
            voxelArray.unsavedChanges = true;

            CustomTextureSelected(customTex);
            EditCustomTexture(customTex);
        });
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

    private void DuplicateCustomTexture(Material material)
    {
        CustomTexture newTex = new CustomTexture(Material.Instantiate(material), layer);
        voxelArray.customTextures[(int)layer].Add(newTex);
        CustomTextureSelected(newTex);
        CategorySelected(CUSTOM_CATEGORY);
        voxelArray.unsavedChanges = true;
        scrollVelocity = new Vector2(0, 1000 * (materials.Count + customTextures.Count));
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
        CategorySelected(CUSTOM_CATEGORY);
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
            customTextures = ReadWorldFile.ReadCustomTextures(path, layer);
            if (customTextures.Count == 0)
                importMessage = "World contains no custom textures for "
                    + (layer == PaintLayer.OVERLAY ? "overlays." : "materials.");
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
