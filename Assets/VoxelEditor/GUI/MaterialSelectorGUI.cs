using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MaterialSelectorGUI : GUIPanel
{
    private static RenderTexture previewTexture;
    private static Material previewMaterial;
    private const int NUM_COLUMNS = 4;
    private const int NUM_COLUMNS_ROOT = 6;
    private const int TEXTURE_MARGIN = 20;
    private const float CATEGORY_BUTTON_HEIGHT = 110;
    private const string PREVIEW_SUFFIX = "_preview";
    private const string CUSTOM_CATEGORY = "CUSTOM";

    public delegate void MaterialSelectHandler(Material material);

    public MaterialSelectHandler handler;
    public string rootDirectory = "Materials";
    public bool allowAlpha = false;
    public bool allowNullMaterial = false;
    public Material highlightMaterial = null; // the current selected material
    public VoxelArrayEditor voxelArray;

    private int tab;
    private string selectedCategory;
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

    private static readonly System.Lazy<GUIStyle> categoryLabelStyle = new System.Lazy<GUIStyle>(() =>
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
            safeRect.width * .5f, safeRect.height * .8f);
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
            Color whitePoint = Color.white;
            if (ResourcesDirectory.materialInfos.ContainsKey(highlightMaterial.name))
            {
                whitePoint = ResourcesDirectory.materialInfos[highlightMaterial.name].whitePoint;
                whitePoint.a = 1.0f;
            }

            colorPicker = gameObject.AddComponent<ColorPickerGUI>();
            colorPicker.enabled = false;
            colorPicker.SetColor(highlightMaterial.GetColor(colorProp) * whitePoint);
            colorPicker.includeAlpha = allowAlpha;
            colorPicker.handler = (Color c) =>
            {
                if (!instance)
                {
                    highlightMaterial = ResourcesDirectory.InstantiateMaterial(highlightMaterial);
                    instance = true;
                }
                // don't believe what they tell you, color values can go above 1.0
                highlightMaterial.SetColor(colorProp, new Color(
                    c.r / whitePoint.r, c.g / whitePoint.g, c.b / whitePoint.b, c.a));
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
        scroll = GUILayout.BeginScrollView(scroll);

        if (selectedCategory != "")
        {
            GUILayout.BeginHorizontal();
            if (ActionBarGUI.ActionBarButton(GUIIconSet.instance.close))
                CategorySelected("");
            if (selectedCategory == CUSTOM_CATEGORY)
            {
                if (ActionBarGUI.ActionBarButton(GUIIconSet.instance.newTexture))
                    ImportTexture();
                if (highlightMaterial != null && CustomTexture.IsCustomTexture(highlightMaterial))
                {
                    if (ActionBarGUI.ActionBarButton(GUIIconSet.instance.draw))
                        EditCustomTexture(new CustomTexture(highlightMaterial, allowAlpha));
                    if (ActionBarGUI.ActionBarButton(GUIIconSet.instance.delete)) { }
                }
            }
            GUILayout.Label(selectedCategory, categoryLabelStyle.Value);
            GUILayout.EndHorizontal();
        }

        Rect rowRect = new Rect();
        int materialColumns = selectedCategory == "" ? NUM_COLUMNS_ROOT : NUM_COLUMNS;
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
            DrawMaterialTexture(material, textureRect, allowAlpha);
        }

        if (categories.Length > 0)
        {
            GUILayout.Label("Categories:");
            int selectDir = GUILayout.SelectionGrid(-1, categories, NUM_COLUMNS,
                categoryButtonStyle.Value);
            if (selectDir != -1)
                CategorySelected(categories[selectDir]);
        }

        GUILayout.EndScrollView();
    }

    private void CategorySelected(string category)
    {
        selectedCategory = category;
        scroll = Vector2.zero;

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

        string currentDirectory = rootDirectory;
        if (category != "")
            currentDirectory += "/" + category;

        var categoriesList = new List<string>();
        if (category == "" && (rootDirectory == "Materials" || rootDirectory == "Overlays"))
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
        instance = false;
        if (handler != null)
            handler(material);
    }

    private void ImportTexture()
    {
        NativeGallery.Permission permission = NativeGallery.GetImageFromGallery((path) => {
            if (path == null)
                return;
            Texture2D texture = NativeGallery.LoadImageAtPath(path, markTextureNonReadable: false);
            if (texture == null)
            {
                DialogGUI.ShowMessageDialog(gameObject, "Error importing image");
                return;
            }
            Debug.Log("Dimensions: " + texture.width + ", " + texture.height);
            Material baseMat = Resources.Load<Material>(
                allowAlpha ? "GameAssets/Overlays/MATTE_overlay" : "GameAssets/Materials/MATTE");
            CustomTexture customTex = CustomTexture.FromBaseMaterial(baseMat, allowAlpha);
            customTex.texture = texture;
            materials.Add(customTex.material);
            voxelArray.unsavedChanges = true;
            handler(customTex.material);
            EditCustomTexture(customTex);
        }, "Select a texture image");

        if (permission != NativeGallery.Permission.Granted)
            DialogGUI.ShowMessageDialog(gameObject, "Please grant N-Space permission to access your photo gallery.");
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

    public static void DrawMaterialTexture(Material mat, Rect rect, bool alpha)
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
                color = new Color(color.r, color.g, color.b, 0.8f);
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

        GL.Begin(GL.QUADS);
        GL.TexCoord2(0, 0);
        GL.Vertex3(0, 0, 0);
        GL.TexCoord2(0, 1);
        GL.Vertex3(0, 1, 0);
        GL.TexCoord2(1, 1);
        GL.Vertex3(1, 1, 0);
        GL.TexCoord2(1, 0);
        GL.Vertex3(1, 0, 0);
        GL.End();

        GL.PopMatrix();
        RenderTexture.active = prevActive;
        GUI.DrawTexture(rect, previewTexture);
    }
}
