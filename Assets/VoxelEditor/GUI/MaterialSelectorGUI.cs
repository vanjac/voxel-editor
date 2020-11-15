using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public enum ColorMode
{
    MATTE, GLOSSY, METAL, UNLIT, GLASS, ADD, MULTIPLY
}

public class MaterialSelectorGUI : GUIPanel
{
    private static Texture2D whiteTexture;
    private const int NUM_COLUMNS = 4;
    private const int TEXTURE_MARGIN = 20;
    private const float CATEGORY_BUTTON_HEIGHT = 110;
    private const string BACK_BUTTON = "Back";
    private const string PREVIEW_SUFFIX = "_preview";

    public delegate void MaterialSelectHandler(Material material);

    public MaterialSelectHandler handler;
    public string rootDirectory = "Materials";
    public bool allowAlpha = false;
    public bool allowNullMaterial = false;
    public Material highlightMaterial = null; // the current selected material

    private int tab;
    private string materialDirectory;
    private List<Material> materials;
    private List<string> materialSubDirectories;
    private ColorPickerGUI colorPicker;
    // created an instance of the selected material?
    private bool instance;

    private static readonly System.Lazy<GUIStyle> directoryButtonStyle = new System.Lazy<GUIStyle>(() =>
    {
        var style = new GUIStyle(GUI.skin.button);
        style.padding.left = 16;
        style.padding.right = 16;
        return style;
    });

    public override void OnEnable()
    {
        showCloseButton = true;
        base.OnEnable();
    }

    public void Start()
    {
        materialDirectory = rootDirectory;
        UpdateMaterialDirectory();
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
            ColorTab();
        else if (colorPicker != null)
        {
            Destroy(colorPicker);
            colorPicker = null;
        }
        if (tab == 0)
            TextureTab();
        else
        {
            scroll = Vector2.zero;
            scrollVelocity = Vector2.zero;
            if (materialDirectory != rootDirectory)
            {
                materialDirectory = rootDirectory;
                UpdateMaterialDirectory();
            }
        }
    }

    private void ColorTab()
    {
        if (highlightMaterial == null || !highlightMaterial.HasProperty("_Color"))
        {
            if (highlightMaterial == null)
                GUILayout.Label("No material selected");
            else
                GUILayout.Label("Can't change color");
            if (colorPicker != null)
            {
                Destroy(colorPicker);
                colorPicker = null;
            }
            return;
        }
        if (colorPicker == null)
        {
            Color whitePoint = Color.white;
            if (ResourcesDirectory.materialInfos.ContainsKey(highlightMaterial.name))
                whitePoint = ResourcesDirectory.materialInfos[highlightMaterial.name].whitePoint;
            whitePoint.a = 1.0f;  // to be safe

            colorPicker = gameObject.AddComponent<ColorPickerGUI>();
            colorPicker.enabled = false;
            colorPicker.SetColor(highlightMaterial.color * whitePoint);
            colorPicker.includeAlpha = allowAlpha;
            colorPicker.handler = (Color c) =>
            {
                if (!instance)
                {
                    highlightMaterial = ResourcesDirectory.InstantiateMaterial(highlightMaterial);
                    instance = true;
                }
                // don't believe what they tell you, color values can go above 1.0
                highlightMaterial.color = new Color(
                    c.r / whitePoint.r, c.g / whitePoint.g, c.b / whitePoint.b, c.a);
                if (handler != null)
                    handler(highlightMaterial);
            };
        }
        colorPicker.WindowGUI();
    }

    private void TextureTab()
    {
        if (materials == null)
            return;
        scroll = GUILayout.BeginScrollView(scroll);
        Rect rowRect = new Rect();
        for (int i = 0; i < materialSubDirectories.Count; i++)
        {
            if (i % NUM_COLUMNS == 0)
                rowRect = GUILayoutUtility.GetRect(0, 999999,
                    CATEGORY_BUTTON_HEIGHT, CATEGORY_BUTTON_HEIGHT,
                    GUILayout.ExpandWidth(true));
            Rect buttonRect = rowRect;
            buttonRect.width = rowRect.width / NUM_COLUMNS;
            buttonRect.x = buttonRect.width * (i % NUM_COLUMNS);
            string subDir = materialSubDirectories[i];
            bool selected;
            if (subDir == BACK_BUTTON)
                // highlight the button
                selected = !GUI.Toggle(buttonRect, true, subDir, directoryButtonStyle.Value);
            else
                selected = GUI.Button(buttonRect, subDir, directoryButtonStyle.Value);
            if (selected)
            {
                scroll = new Vector2(0, 0);
                MaterialDirectorySelected(materialSubDirectories[i]);
            }
        }
        string highlightName = "", previewName = "";
        if (highlightMaterial != null)
        {
            highlightName = highlightMaterial.name;
            previewName = highlightName + PREVIEW_SUFFIX;
        }
        for (int i = 0; i < materials.Count; i++)
        {
            if (i % NUM_COLUMNS == 0)
                rowRect = GUILayoutUtility.GetAspectRect(NUM_COLUMNS);
            Rect buttonRect = rowRect;
            buttonRect.width = buttonRect.height;
            buttonRect.x = buttonRect.width * (i % NUM_COLUMNS);
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
        GUILayout.EndScrollView();
    }

    void UpdateMaterialDirectory()
    {
        materialSubDirectories = new List<string>();
        if (materialDirectory != rootDirectory)
            materialSubDirectories.Add(BACK_BUTTON);
        materials = new List<Material>();
        foreach (MaterialInfo dirEntry in ResourcesDirectory.materialInfos.Values)
        {
            if (dirEntry.parent != materialDirectory)
                continue;
            if (dirEntry.name.StartsWith("$"))
                continue; // special alternate materials for game
            if (dirEntry.isDirectory)
                materialSubDirectories.Add(dirEntry.name);
            else
            {
                if (dirEntry.name.EndsWith(PREVIEW_SUFFIX))
                    materials.RemoveAt(materials.Count - 1); // special preview material which replaces the previous
                materials.Add(ResourcesDirectory.LoadMaterial(dirEntry));
            }
        }

        AssetManager.UnusedAssets();
    }

    private void MaterialDirectorySelected(string name)
    {
        if (name == BACK_BUTTON)
        {
            if (materialDirectory.Length != 0)
                materialDirectory = materialDirectory.Substring(0, materialDirectory.LastIndexOf("/"));
            UpdateMaterialDirectory();
            return;
        }
        else
        {
            if (materialDirectory.Length == 0)
                materialDirectory = name;
            else
                materialDirectory += "/" + name;
            UpdateMaterialDirectory();
        }
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

    public static void DrawMaterialTexture(Material mat, Rect rect, bool alpha)
    {
        if (mat == null)
            return;
        if (whiteTexture == null)
        {
            whiteTexture = new Texture2D(1, 1);
            whiteTexture.SetPixel(0, 0, Color.white);
            whiteTexture.Apply();
        }
        Rect texCoords = new Rect(Vector2.zero, Vector2.one);
        Texture texture = whiteTexture;
        if (mat.HasProperty("_MainTex") && mat.mainTexture != null)
        {
            texture = mat.mainTexture;
            texCoords = new Rect(Vector2.zero, mat.mainTextureScale);
        }
        else if (mat.HasProperty("_ColorControl"))
            // water shader
            texture = mat.GetTexture("_ColorControl");
        else if (mat.HasProperty("_FrontTex"))
            // skybox
            texture = mat.GetTexture("_FrontTex");

        Color baseColor = GUI.color;
        if (mat.HasProperty("_Color"))
        {
            if (GUI.color.a > 1)
                // fixes transparent colors becoming opaque while scrolling
                GUI.color = new Color(GUI.color.r, GUI.color.g, GUI.color.b, 1);
            GUI.color *= mat.color;
        }
        else if (texture == whiteTexture)
        {
            // no color or texture
            GUI.color = baseColor;
            return;
        }
        GUI.DrawTextureWithTexCoords(rect, texture, texCoords, alpha);
        GUI.color = baseColor;
    }
}
