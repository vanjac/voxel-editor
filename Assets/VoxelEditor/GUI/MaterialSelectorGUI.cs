using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialSelectorGUI : GUIPanel
{
    private static RenderTexture previewTexture;
    private static Material previewMaterial;
    private const int NUM_COLUMNS = 4;
    private const int NUM_COLUMNS_ROOT = 6;
    private const int TEXTURE_MARGIN = 20;
    private const string PREVIEW_SUFFIX = "_preview";
    private const string CUSTOM_CATEGORY = "CUSTOM";
    private const string WORLD_LIST_CATEGORY = "Import from world...";

    public System.Action<Material> handler;
    public string rootDirectory = "Materials";
    public bool isOverlay = false;
    public bool allowNullMaterial = false;
    public bool customTextureBase = false;
    public Material highlightMaterial = null; // the current selected material
    public VoxelArrayEditor voxelArray;

    private enum Page { TEXTURE, COLOR };
    private Page page = Page.TEXTURE;
    private string selectedCategory;
    private bool importFromWorld, loadingWorld;
    private string importMessage = null;
    private List<Material> materials;
    private string[] categories;
    private ColorPickerGUI colorPicker;
    // created an instance of the selected material?
    private bool instance;
    private Color whitePoint;  // white point only applies in TINT style
    private bool showColorStyle;
    private ResourcesDirectory.ColorStyle colorStyle;

    private static readonly System.Lazy<GUIStyle> categoryButtonStyle = new System.Lazy<GUIStyle>(() =>
    {
        var style = new GUIStyle(StyleSet.buttonLarge);
        style.padding.left = 0;
        style.padding.right = 0;
        return style;
    });

    public static readonly System.Lazy<GUIStyle> categoryLabelStyle = new System.Lazy<GUIStyle>(() =>
    {
        var style = new GUIStyle(StyleSet.labelTitle);
        style.alignment = TextAnchor.MiddleCenter;
        style.fixedHeight = StyleSet.buttonLarge.fixedHeight;
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

    public override Rect GetRect(Rect safeRect, Rect screenRect) =>
        GUIUtils.CenterRect(safeRect.center.x, safeRect.center.y,
            safeRect.width * .5f, safeRect.height * .8f, maxWidth: 1280);

    public override void WindowGUI()
    {
        if (page == Page.COLOR)
        {
            ColorPage();
        }
        else if (colorPicker != null)
        {
            Destroy(colorPicker);
            colorPicker = null;
        }

        if (page == Page.TEXTURE)
            TexturePage();
        else
            scrollVelocity = Vector2.zero;
    }

    private void ColorPage()
    {
        GUILayout.BeginHorizontal();
        if (ActionBarGUI.ActionBarButton(IconSet.close))
            page = Page.TEXTURE;
        GUILayout.Label("Adjust color", categoryLabelStyle.Value);
        GUILayout.EndHorizontal();

        string colorProp = ResourcesDirectory.MaterialColorProperty(highlightMaterial);
        if (colorPicker == null)
        {
            whitePoint = Color.white;
            showColorStyle = false;
            colorStyle = ResourcesDirectory.ColorStyle.PAINT;  // ignore white point by default
            if (!customTextureBase &&
                ResourcesDirectory.materialInfos.TryGetValue(highlightMaterial.name, out var info))
            {
                whitePoint = info.whitePoint;
                whitePoint.a = 1.0f;
                showColorStyle = info.supportsColorStyles;
                colorStyle = ResourcesDirectory.GetMaterialColorStyle(highlightMaterial);
            }

            colorPicker = gameObject.AddComponent<ColorPickerGUI>();
            colorPicker.enabled = false;
            Color currentColor = highlightMaterial.GetColor(colorProp);
            if (colorStyle == ResourcesDirectory.ColorStyle.TINT)
                currentColor *= whitePoint;
            colorPicker.SetColor(currentColor);
            colorPicker.includeAlpha = isOverlay;
            colorPicker.handler = (Color c) =>
            {
                MakeInstance();
                // don't believe what they tell you, color values can go above 1.0
                if (colorStyle == ResourcesDirectory.ColorStyle.TINT)
                    c = new Color(c.r / whitePoint.r, c.g / whitePoint.g, c.b / whitePoint.b, c.a);
                highlightMaterial.SetColor(colorProp, c);
                if (handler != null)
                    handler(highlightMaterial);
            };
        }
        colorPicker.WindowGUI();
        if (showColorStyle)
        {
            var newStyle = (ResourcesDirectory.ColorStyle)GUILayout.SelectionGrid((int)colorStyle,
                new string[] { "Tint", "Paint" }, 2);
            if (newStyle != colorStyle)
            {
                colorStyle = newStyle;
                MakeInstance();
                ResourcesDirectory.SetMaterialColorStyle(highlightMaterial, newStyle);
                colorPicker.CallHandler();  // update white point and call material handler also
            }
        }
    }

    private void TexturePage()
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
        if (selectedCategory == "" && !importFromWorld)
            GUIUtils.ShowDisabled();
        if (ActionBarGUI.ActionBarButton(IconSet.close))
            BackButton();
        GUI.enabled = wasEnabled;
        GUI.color = baseColor;

        if (selectedCategory == CUSTOM_CATEGORY)
        {
            if (ActionBarGUI.ActionBarButton(IconSet.newTexture))
                ImportTextureFromPhotos();
            if (ActionBarGUI.ActionBarButton(IconSet.worldImport))
                CategorySelected(WORLD_LIST_CATEGORY);
            if (highlightMaterial != null && CustomTexture.IsCustomTexture(highlightMaterial))
            {
                if (ActionBarGUI.ActionBarButton(IconSet.copy))
                    DuplicateCustomTexture();
                if (ActionBarGUI.ActionBarButton(IconSet.draw))
                    EditCustomTexture(new CustomTexture(highlightMaterial, isOverlay));
                if (ActionBarGUI.ActionBarButton(IconSet.delete))
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

        wasEnabled = GUI.enabled;
        baseColor = GUI.color;
        if (highlightMaterial == null || CustomTexture.IsCustomTexture(highlightMaterial)
            || ResourcesDirectory.MaterialColorProperty(highlightMaterial) == null)
        {
            GUIUtils.ShowDisabled();
        }
        if (ActionBarGUI.ActionBarButton(IconSet.color))
            page = Page.COLOR;
        GUI.enabled = wasEnabled;
        GUI.color = baseColor;

        if (allowNullMaterial && selectedCategory == "")
        {
            if (highlightMaterial != null && ActionBarGUI.ActionBarButton(IconSet.no))
                MaterialSelected(null);
            else if (highlightMaterial == null)
                ActionBarGUI.HighlightedActionBarButton(IconSet.no);
        }

        GUILayout.EndHorizontal();

        scroll = GUILayout.BeginScrollView(scroll);
        if (importFromWorld && (importMessage != null))
            GUILayout.Label(importMessage);

        Rect rowRect = new Rect();
        int materialColumns = categories.Length > 0 ? NUM_COLUMNS_ROOT : NUM_COLUMNS;
        string highlightName = highlightMaterial?.name;
        string previewName = (highlightName != null) ? (highlightName + PREVIEW_SUFFIX) : null;
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
            if (material?.name == highlightName || material?.name == previewName)
                // highlight the button
                selected = !GUI.Toggle(buttonRect, true, "", GUI.skin.button);
            else
                selected = GUI.Button(buttonRect, "");
            if (selected)
                MaterialSelected(material);
            DrawMaterialTexture(material, textureRect, isOverlay);
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
            if (rootDirectory == "Overlays")
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

        string currentDirectory = rootDirectory;
        if (category != "")
            currentDirectory += "/" + category;

        var categoriesList = new List<string>();
        if (!customTextureBase && category == ""
                && (rootDirectory == "Materials" || rootDirectory == "Overlays"))
            categoriesList.Add(CUSTOM_CATEGORY);
        materials = new List<Material>();
        foreach (MaterialInfo dirEntry in ResourcesDirectory.materialInfos.Values)
        {
            if (dirEntry.parent != currentDirectory)
                continue;
            if (dirEntry.name.StartsWith("$"))
                continue; // special alternate materials for game
            if (dirEntry.isDirectory)
                categoriesList.Add(dirEntry.name);
            else
            {
                if (dirEntry.name.EndsWith(PREVIEW_SUFFIX))
                    materials.RemoveAt(materials.Count - 1); // special preview material which replaces the previous
                materials.Add(ResourcesDirectory.LoadMaterial(dirEntry));
            }
        }
        categories = categoriesList.ToArray();

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
        NativeGalleryWrapper.ImportTexture((Texture2D texture) =>
        {
            if (texture == null)
                return;

            Material baseMat = ResourcesDirectory.InstantiateMaterial(Resources.Load<Material>(
                isOverlay ? "GameAssets/Overlays/MATTE_overlay" : "GameAssets/Materials/MATTE"));
            baseMat.color = new Color(1, 1, 1, baseMat.color.a);
            CustomTexture customTex = CustomTexture.FromBaseMaterial(baseMat, isOverlay);
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
        CustomTexture customTex = new CustomTexture(highlightMaterial, isOverlay);
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
            materials = ReadWorldFile.ReadCustomTextures(path, isOverlay);
            if (materials.Count == 0)
                importMessage = "World contains no custom textures for " + (isOverlay ? "overlays." : "materials.");
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

    public static void DrawMaterialTexture(Material mat, Rect rect, bool alpha)
    {
        DrawMaterialTexture(mat, rect, alpha, Vector2.right, Vector2.up);
    }

    public static void DrawMaterialTexture(Material mat, Rect rect, bool alpha, Vector2 u_vec, Vector2 v_vec)
    {
        if (mat == null)
            return;
        if (previewTexture == null)
            previewTexture = new RenderTexture(256, 256, 0, RenderTextureFormat.ARGB32);
        if (!previewTexture.IsCreated())
            previewTexture.Create();
        if (previewMaterial == null)
            previewMaterial = new Material(Shader.Find("Unlit/MaterialPreview"));  // TODO make sure shader is included

        RenderTexture prevActive = RenderTexture.active;
        RenderTexture.active = previewTexture;

        GL.PushMatrix();
        GL.LoadOrtho();
        GL.Clear(false, true, Color.clear);

        previewMaterial.CopyPropertiesFromMaterial(mat);
        string colorProp = ResourcesDirectory.MaterialColorProperty(mat);
        if (colorProp == null)
            previewMaterial.color = Color.white;
        else
        {
            Color color = mat.GetColor(colorProp);
            if (color.a == 0.0f)
                color = new Color(color.r, color.g, color.b, 0.6f);
            previewMaterial.color = color;
        }
        if (!mat.HasProperty("_BumpMap"))
            previewMaterial.SetTexture("_BumpMap", Texture2D.normalTexture);
        if (!mat.HasProperty("_MainTex"))
        {
            if (mat.HasProperty("_FrontTex"))  // 6-sided skybox
            {
                previewMaterial.mainTexture = mat.GetTexture("_FrontTex");
                previewMaterial.color *= 2;
            }
            else
                previewMaterial.mainTexture = Texture2D.whiteTexture;
        }
        if (mat.HasProperty("_WaveScale"))  // water
            previewMaterial.mainTextureScale = Vector2.one * mat.GetFloat("_WaveScale");
        previewMaterial.SetPass(0);

        Vector2 uv;
        GL.Begin(GL.QUADS);
        uv = (Vector2.one - u_vec - v_vec) / 2;
        GL.TexCoord2(uv.x, uv.y);
        GL.Vertex3(0, 0, 0);
        uv = (Vector2.one - u_vec + v_vec) / 2;
        GL.TexCoord2(uv.x, uv.y);
        GL.Vertex3(0, 1, 0);
        uv = (Vector2.one + u_vec + v_vec) / 2;
        GL.TexCoord2(uv.x, uv.y);
        GL.Vertex3(1, 1, 0);
        uv = (Vector2.one + u_vec - v_vec) / 2;
        GL.TexCoord2(uv.x, uv.y);
        GL.Vertex3(1, 0, 0);
        GL.End();

        GL.PopMatrix();
        RenderTexture.active = prevActive;
        GUI.DrawTexture(rect, previewTexture);
    }
}
