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
    private const string PREVIEW_SUFFIX = "_preview";
    private const string CUSTOM_CATEGORY = " CUSTOM "; // leading space for sorting order (sorry)
    private const string WORLD_LIST_CATEGORY = "Import from world...";

    public delegate void MaterialSelectHandler(Material material);

    public MaterialSelectHandler handler;
    public PaintLayer layer = PaintLayer.MATERIAL;
    public bool allowNullMaterial = false;
    public bool customTextureBase = false;
    public Material highlightMaterial = null; // the current selected material
    public VoxelArrayEditor voxelArray;

    private int tab;
    private string selectedCategory;
    private bool importFromWorld, loadingWorld;
    private string importMessage = null;
    private List<Material> materials;
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
        tab = GUILayout.SelectionGrid(tab, new string[] { "Texture", "Color" }, 2);
        TutorialGUI.ClearHighlight();
        if (allowNullMaterial && GUILayout.Button("Clear", GUILayout.ExpandWidth(false)))
            MaterialSelected(null);
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
        if (highlightMaterial == null)
        {
            GUILayout.Label("No texture selected");
            return false;
        }
        if (CustomTexture.IsCustomTexture(highlightMaterial))
        {
            GUILayout.Label("Can't change color of custom texture");
            return false;
        }
        string colorProp = ResourcesDirectory.MaterialColorProperty(highlightMaterial);
        if (colorProp == null)
        {
            GUILayout.Label("Can't change color of this texture");
            return false;
        }
        if (colorPicker == null)
        {
            colorPicker = gameObject.AddComponent<ColorPickerGUI>();
            colorPicker.enabled = false;
            Color currentColor = highlightMaterial.GetColor(colorProp);
            colorPicker.SetColor(currentColor);
            colorPicker.includeAlpha = layer == PaintLayer.OVERLAY;
            colorPicker.handler = (Color c) =>
            {
                MakeInstance();
                highlightMaterial.SetColor(colorProp, c);
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
        scroll = GUILayout.BeginScrollView(scroll);

        if (selectedCategory != "")
        {
            GUILayout.BeginHorizontal();
            if (ActionBarGUI.ActionBarButton(GUIIconSet.instance.close))
                BackButton();
            // prevent from expanding window
            GUIUtils.BeginHorizontalClipped(GUILayout.ExpandHeight(false));
            if (selectedCategory == CUSTOM_CATEGORY)
            {
                if (ActionBarGUI.ActionBarButton(GUIIconSet.instance.newTexture))
                    ImportTextureFromPhotos();
                if (ActionBarGUI.ActionBarButton(GUIIconSet.instance.worldImport))
                    CategorySelected(WORLD_LIST_CATEGORY);
                if (highlightMaterial != null && CustomTexture.IsCustomTexture(highlightMaterial))
                {
                    if (ActionBarGUI.ActionBarButton(GUIIconSet.instance.copy))
                        DuplicateCustomTexture();
                    if (ActionBarGUI.ActionBarButton(GUIIconSet.instance.draw))
                        EditCustomTexture(new CustomTexture(highlightMaterial, layer));
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
            GUILayout.Label(selectedCategory, categoryLabelStyle.Value);
            GUIUtils.EndHorizontalClipped();
            GUILayout.EndHorizontal();
        }

        if (importFromWorld && (importMessage != null))
            GUILayout.Label(importMessage);

        Rect rowRect = new Rect();
        int materialColumns = categories.Length > 0 ? NUM_COLUMNS_ROOT : NUM_COLUMNS;
        string highlightName = "", previewName = "";
        if (highlightMaterial != null)
        {
            highlightName = highlightMaterial.name;
            previewName = highlightName + PREVIEW_SUFFIX;
        }
        for (int i = 0; i < materials.Count; i++)
        {
            if (i % materialColumns == 0)
                rowRect = GUILayoutUtility.GetAspectRect(materialColumns);
            Rect buttonRect = rowRect;
            buttonRect.width = buttonRect.height;
            buttonRect.x = buttonRect.width * (i % materialColumns);
            Rect textureRect = new Rect(
                buttonRect.xMin + TEXTURE_MARGIN, buttonRect.yMin + TEXTURE_MARGIN,
                buttonRect.width - TEXTURE_MARGIN * 2, buttonRect.height - TEXTURE_MARGIN * 2);
            Material material = materials[i];
            bool selected;
            if (material != null && (material.name == highlightName
                    || material.name == previewName))
                // highlight the button
                selected = !GUI.Toggle(buttonRect, true, "", GUI.skin.button);
            else
                selected = GUI.Button(buttonRect, "");
            if (selected)
                MaterialSelected(material);
            DrawMaterialTexture(material, textureRect);
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

    private void MakeInstance()
    {
        if (!instance)
        {
            //Debug.Log("instantiate");
            highlightMaterial = ResourcesDirectory.InstantiateMaterial(highlightMaterial);
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

        if (category == CUSTOM_CATEGORY)
        {
            categories = new string[0];
            if (layer == PaintLayer.OVERLAY)
                materials = voxelArray.customOverlays;
            else
                materials = voxelArray.customMaterials;
            AssetManager.UnusedAssets();
            return;
        }
        else if (category == WORLD_LIST_CATEGORY)
        {
            materials = new List<Material>();
            var worldNames = new List<string>();
            WorldFiles.ListWorlds(new List<string>(), worldNames);
            categories = worldNames.ToArray();
            AssetManager.UnusedAssets();
            return;
        }
        else if (importFromWorld)
        {
            categories = new string[0];
            materials = new List<Material>();
            // TODO :(
            StartCoroutine(LoadWorldCoroutine(WorldFiles.GetNewWorldPath(category)));
            return;
        }

        var categoriesSet = new SortedSet<string>();
        if (!customTextureBase && category == ""
                && (layer == PaintLayer.MATERIAL || layer == PaintLayer.OVERLAY))
            categoriesSet.Add(CUSTOM_CATEGORY);
        materials = new List<Material>();
        foreach (MaterialInfo dirEntry in ResourcesDirectory.materialInfos.Values)
        {
            if (dirEntry.layer != layer)
                continue;
            if (dirEntry.category != category)
            {
                if (category == "")
                    categoriesSet.Add(dirEntry.category);
                continue;
            }
            if (dirEntry.name.StartsWith("$"))
                continue; // special alternate materials for game
            if (dirEntry.name.EndsWith(PREVIEW_SUFFIX))
                materials.RemoveAt(materials.Count - 1); // special preview material which replaces the previous
            materials.Add(ResourcesDirectory.LoadMaterial(dirEntry));
        }
        categories = new string[categoriesSet.Count];
        categoriesSet.CopyTo(categories);

        AssetManager.UnusedAssets();
    }

    private void MaterialSelected(Material material)
    {
        if (material != null && material.name.EndsWith(PREVIEW_SUFFIX))
        {
            string newName = material.name.Substring(0, material.name.Length - PREVIEW_SUFFIX.Length);
            material = ResourcesDirectory.FindMaterial(newName, true);
        }
        highlightMaterial = material;
        if (importFromWorld)
        {
            // jank
            importFromWorld = false;
            CategorySelected(CUSTOM_CATEGORY);
            DuplicateCustomTexture();  // add to custom textures, will call MaterialSelected again
            return;  // don't call handler
        }
        instance = false;
        if (customTextureBase && highlightMaterial != null)
        {
            // reset color to white
            string colorProp = ResourcesDirectory.MaterialColorProperty(highlightMaterial);
            if (colorProp != null)
            {
                Color prevColor = highlightMaterial.GetColor(colorProp);
                Color newColor = new Color(1, 1, 1, prevColor.a);
                if (newColor != prevColor)
                {
                    MakeInstance();
                    highlightMaterial.SetColor(colorProp, newColor);
                }
            }
        }
        if (handler != null)
            handler(highlightMaterial);
    }

    private void ImportTextureFromPhotos()
    {
        NativeGalleryWrapper.ImportTexture((Texture2D texture) => {
            if (texture == null)
                return;

            Material baseMat = ResourcesDirectory.InstantiateMaterial(Resources.Load<Material>(
                layer == PaintLayer.OVERLAY ? "GameAssets/Overlays/MATTE_overlay" : "GameAssets/Materials/MATTE"));
            baseMat.color = new Color(1, 1, 1, baseMat.color.a);
            CustomTexture customTex = CustomTexture.FromBaseMaterial(baseMat, layer);
            customTex.texture = texture;

            materials.Add(customTex.material);
            voxelArray.unsavedChanges = true;

            MaterialSelected(customTex.material);
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

    private void DuplicateCustomTexture()
    {
        Material newMat = CustomTexture.Clone(highlightMaterial);
        materials.Add(newMat);
        MaterialSelected(newMat);
        voxelArray.unsavedChanges = true;
        scrollVelocity = new Vector2(0, 1000 * materials.Count);
    }

    private void DeleteCustomTexture()
    {
        CustomTexture customTex = new CustomTexture(highlightMaterial, layer);
        voxelArray.ReplaceMaterial(highlightMaterial, customTex.baseMat);
        if (!materials.Remove(highlightMaterial))
            Debug.LogError("Error removing material");
        MaterialSelected(customTex.baseMat);
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
            materials = ReadWorldFile.ReadCustomTextures(path, layer);
            if (materials.Count == 0)
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
        string colorProp = ResourcesDirectory.MaterialColorProperty(mat);
        if (colorProp != null)
        {
            Color color = mat.GetColor(colorProp);
            if (color.a == 0.0f)
                color = new Color(color.r, color.g, color.b, 0.6f);
            GUI.color *= color;
        }

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
