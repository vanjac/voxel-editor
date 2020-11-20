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
    private static RenderTexture previewTexture;
    private static Material previewMaterial;
    private const int NUM_COLUMNS = 4;
    private const int NUM_COLUMNS_ROOT = 6;
    private const int TEXTURE_MARGIN = 20;
    private const float CATEGORY_BUTTON_HEIGHT = 110;
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
    private string[] materialSubDirectories;
    private ColorPickerGUI colorPicker;
    // created an instance of the selected material?
    private bool instance;

    private static readonly System.Lazy<GUIStyle> directoryButtonStyle = new System.Lazy<GUIStyle>(() =>
    {
        var style = new GUIStyle(GUIStyleSet.instance.buttonLarge);
        style.padding.left = 0;
        style.padding.right = 0;
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
            scrollVelocity = Vector2.zero;
        }
    }

    private void ColorTab()
    {
        if (highlightMaterial == null || !highlightMaterial.HasProperty("_Color"))
        {
            if (highlightMaterial == null)
                GUILayout.Label("No texture selected");
            else
                GUILayout.Label("Can't change color of this texture");
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
            {
                whitePoint = ResourcesDirectory.materialInfos[highlightMaterial.name].whitePoint;
                whitePoint.a = 1.0f;
            }

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

        if (materialDirectory != rootDirectory)
        {
            if (ActionBarGUI.ActionBarButton(GUIIconSet.instance.close))
            {
                if (materialDirectory.Length != 0)
                    materialDirectory = materialDirectory.Substring(0, materialDirectory.LastIndexOf("/"));
                UpdateMaterialDirectory();
            }
        }

        Rect rowRect = new Rect();
        int materialColumns = materialDirectory == rootDirectory ? NUM_COLUMNS_ROOT : NUM_COLUMNS;
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

        if (materialSubDirectories.Length > 0)
        {
            GUILayout.Label("Categories:");
            int selectDir = GUILayout.SelectionGrid(-1, materialSubDirectories, NUM_COLUMNS,
                directoryButtonStyle.Value);
            if (selectDir != -1)
            {
                scroll = new Vector2(0, 0);
                MaterialDirectorySelected(materialSubDirectories[selectDir]);
            }
        }

        GUILayout.EndScrollView();
    }

    void UpdateMaterialDirectory()
    {
        var materialDirectoriesList = new List<string>();
        materials = new List<Material>();
        foreach (MaterialInfo dirEntry in ResourcesDirectory.materialInfos.Values)
        {
            if (dirEntry.parent != materialDirectory)
                continue;
            if (dirEntry.name.StartsWith("$"))
                continue; // special alternate materials for game
            if (dirEntry.isDirectory)
                materialDirectoriesList.Add(dirEntry.name);
            else
            {
                if (dirEntry.name.EndsWith(PREVIEW_SUFFIX))
                    materials.RemoveAt(materials.Count - 1); // special preview material which replaces the previous
                materials.Add(ResourcesDirectory.LoadMaterial(dirEntry));
            }
        }
        materialSubDirectories = materialDirectoriesList.ToArray();

        AssetManager.UnusedAssets();
    }

    private void MaterialDirectorySelected(string name)
    {
        if (materialDirectory.Length == 0)
            materialDirectory = name;
        else
            materialDirectory += "/" + name;
        UpdateMaterialDirectory();
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
        if (!mat.HasProperty("_Color"))
            previewMaterial.color = Color.white;
        else if (mat.color.a == 0.0f)
            previewMaterial.color = new Color(mat.color.r, mat.color.g, mat.color.b, 0.8f);
        if (!mat.HasProperty("_BumpMap"))
            previewMaterial.SetTexture("_BumpMap", Texture2D.normalTexture);
        if (!mat.HasProperty("_MainTex"))
        {
            if (mat.HasProperty("_FrontTex"))  // skybox
                previewMaterial.mainTexture = mat.GetTexture("_FrontTex");
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
