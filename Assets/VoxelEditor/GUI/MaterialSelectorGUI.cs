using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MaterialSelectorGUI : GUIPanel
{
    public delegate void MaterialSelectHandler(Material material);

    public MaterialSelectHandler handler;
    public string materialDirectory = "GameAssets/Materials";
    public bool allowNullMaterial = false;

    List<string> materialNames;
    List<Texture> materialPreviews;
    List<string> materialSubDirectories;

    void Start()
    {
        UpdateMaterialDirectory();
    }

    public override Rect GetRect(float width, float height)
    {
        return new Rect(width - height / 2, 0, height / 2, height);
    }

    public override void WindowGUI()
    {
        if (materialPreviews == null)
            return;
        scroll = GUILayout.BeginScrollView(scroll);
        if (allowNullMaterial)
        {
            if (GUILayout.Button("Clear"))
            {
                MaterialSelected(null);
            }
        }
        for (int i = 0; i < materialSubDirectories.Count; i++)
        {
            string subDir = materialSubDirectories[i];
            if (GUILayout.Button(subDir))
            {
                scroll = new Vector2(0, 0);
                MaterialDirectorySelected(materialSubDirectories[i]);
            }
        }
        for (int i = 0; i < materialPreviews.Count; i++)
        {
            Texture materialPreview = materialPreviews[i];
            Rect buttonRect = GUILayoutUtility.GetAspectRect(1.0f);
            Rect textureRect = new Rect(buttonRect.xMin + 40, buttonRect.yMin + 40,
                buttonRect.width - 80, buttonRect.height - 80);
            if (GUI.Button(buttonRect, ""))
            {
                MaterialSelected(materialNames[i]);
            }
            GUI.DrawTexture(textureRect, materialPreview, ScaleMode.ScaleToFit, false);
        }
        GUILayout.EndScrollView();
    }

    void UpdateMaterialDirectory()
    {
        materialSubDirectories = new List<string>();
        materialSubDirectories.Add("..");
        materialNames = new List<string>();
        materialPreviews = new List<Texture>();
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
            {
                materialNames.Add(Path.GetFileNameWithoutExtension(newDirEntry));
                Material material = ResourcesDirectory.GetMaterial(newDirEntry);
                if (material == null)
                {
                    materialPreviews.Add(null);
                    continue;
                }

                Texture previewTexture = null;
                Color color = Color.white;

                if (material.mainTexture != null)
                    previewTexture = material.mainTexture;
                else if (material.HasProperty("_Color"))
                    color = material.color;
                else if (material.HasProperty("_ColorControl"))
                    // water shader
                    previewTexture = material.GetTexture("_ColorControl");
                else if (material.HasProperty("_FrontTex"))
                    // skybox
                    previewTexture = material.GetTexture("_FrontTex");
                if (previewTexture == null)
                {
                    Texture2D solidColorTexture = new Texture2D(1, 1);
                    solidColorTexture.SetPixel(0, 0, color);
                    solidColorTexture.Apply();
                    previewTexture = solidColorTexture;
                }
                materialPreviews.Add(previewTexture);
            }
        }

        Resources.UnloadUnusedAssets();
    }

    void MaterialDirectorySelected(string name)
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

    void MaterialSelected(string name)
    {
        Material material = null;
        if (name != null)
            material = ResourcesDirectory.GetMaterial(materialDirectory + "/" + name);
        if (handler != null)
            handler(material);
        Destroy(this);
    }
}
