using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MaterialSelectorGUI : GUIPanel
{
    private static Texture2D whiteTexture;

    public delegate void MaterialSelectHandler(Material material);

    public MaterialSelectHandler handler;
    public string materialDirectory = "GameAssets/Materials";
    public bool allowNullMaterial = false;
    public bool closeOnSelect = true;

    List<Material> materials;
    List<string> materialSubDirectories;

    public void Start()
    {
        UpdateMaterialDirectory();
    }

    public override Rect GetRect(float width, float height)
    {
        return new Rect(width * .25f, height * .1f, width * .5f, height * .8f);
    }

    public override void WindowGUI()
    {
        if (materials == null)
            return;
        scroll = GUILayout.BeginScrollView(scroll);
        if (allowNullMaterial)
            if (GUILayout.Button("Clear"))
                MaterialSelected(null);
        for (int i = 0; i < materialSubDirectories.Count; i++)
        {
            string subDir = materialSubDirectories[i];
            if (GUILayout.Button(subDir))
            {
                scroll = new Vector2(0, 0);
                MaterialDirectorySelected(materialSubDirectories[i]);
            }
        }
        for (int i = 0; i < materials.Count; i++)
        {
            Rect buttonRect = GUILayoutUtility.GetAspectRect(1.0f);
            Rect textureRect = new Rect(buttonRect.xMin + 40, buttonRect.yMin + 40,
                buttonRect.width - 80, buttonRect.height - 80);
            if (GUI.Button(buttonRect, ""))
                MaterialSelected(materials[i]);
            DrawMaterialTexture(materials[i], textureRect, false);
        }
        GUILayout.EndScrollView();
    }

    void UpdateMaterialDirectory()
    {
        materialSubDirectories = new List<string>();
        materialSubDirectories.Add("..");
        materials = new List<Material>();
        foreach (string dirEntry in ResourcesDirectory.dirList)
        {
            if (dirEntry.Length <= 2)
                continue;
            string newDirEntry = dirEntry.Substring(2);
            if (Path.GetFileName(newDirEntry).StartsWith("$"))
                continue; // special alternate materials for game
            string directory = Path.GetDirectoryName(newDirEntry);
            if (directory != materialDirectory)
                continue;
            string extension = Path.GetExtension(newDirEntry);
            if (extension == "")
                materialSubDirectories.Add(Path.GetFileName(newDirEntry));
            else if (extension == ".mat")
                materials.Add(ResourcesDirectory.GetMaterial(newDirEntry));
        }

        Resources.UnloadUnusedAssets();
    }

    private void MaterialDirectorySelected(string name)
    {
        if (name == "..")
        {
            if (materialDirectory.Trim() != "")
                materialDirectory = Path.GetDirectoryName(materialDirectory);
            UpdateMaterialDirectory();
            return;
        }
        else
        {
            if (materialDirectory.Trim() == "")
                materialDirectory = name;
            else
                materialDirectory += "/" + name;
            UpdateMaterialDirectory();
        }
    }

    private void MaterialSelected(Material material)
    {
        if (handler != null)
            handler(material);
        if (closeOnSelect)
            Destroy(this);
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
        if (mat.mainTexture != null)
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
            GUI.color *= mat.color;
        GUI.DrawTextureWithTexCoords(rect, texture, texCoords, alpha);
        GUI.color = baseColor;
    }
}
