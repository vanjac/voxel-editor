using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MaterialSelectorGUI : GUIPanel
{
    public VoxelArray voxelArray;

    List<string> materialNames;
    List<Texture> materialPreviews;
    string materialDirectory = "GameAssets/Materials";
    List<string> materialSubDirectories;

    void OnEnable()
    {
        UpdateMaterialDirectory();
        depth = -1;
        base.OnEnable();
    }

    void OnGUI()
    {
        base.OnGUI();

        panelRect = new Rect(scaledScreenWidth - 180, 0, 180, PropertiesGUI.targetHeight);

        GUI.Box(panelRect, "Assign Material");

        if (materialPreviews == null)
            return;
        Rect scrollBox = new Rect(panelRect.xMin, panelRect.yMin + 25, panelRect.width, panelRect.height - 25);
        float scrollAreaWidth = panelRect.width - 1;
        float buttonWidth = scrollAreaWidth - 20;
        float scrollAreaHeight = materialSubDirectories.Count * 25 + materialPreviews.Count * buttonWidth;
        Rect scrollArea = new Rect(0, 0, scrollAreaWidth, scrollAreaHeight);
        scroll = GUI.BeginScrollView(scrollBox, scroll, scrollArea);
        float y = 0;
        for (int i = 0; i < materialSubDirectories.Count; i++)
        {
            string subDir = materialSubDirectories[i];
            if (GUI.Button(new Rect(10, y, buttonWidth, 20), subDir))
            {
                scroll = new Vector2(0, 0);
                MaterialDirectorySelected(materialSubDirectories[i]);
            }
            y += 25;
        }
        for (int i = 0; i < materialPreviews.Count; i++)
        {
            Texture materialPreview = materialPreviews[i];
            if (GUI.Button(new Rect(10, y, buttonWidth, buttonWidth), materialPreview))
            {
                MaterialSelected(materialNames[i]);
            }
            y += buttonWidth;
        }
        GUI.EndScrollView();
    }

    void UpdateMaterialDirectory()
    {
        Debug.Log(materialDirectory);
        materialSubDirectories = new List<string>();
        materialSubDirectories.Add("..");
        materialNames = new List<string>();
        materialPreviews = new List<Texture>();
        foreach (string dirEntry in ResourcesDirectory.dirList)
        {
            if (dirEntry.Length <= 2)
                continue;
            string newDirEntry = dirEntry.Substring(2);
            string directory = Path.GetDirectoryName(newDirEntry);
            if (directory != materialDirectory)
                continue;
            string extension = Path.GetExtension(newDirEntry);
            if (extension == "")
                materialSubDirectories.Add(Path.GetFileName(newDirEntry));
            else if (extension == ".mat")
            {
                materialNames.Add(Path.GetFileNameWithoutExtension(newDirEntry));
                Material material = Resources.Load<Material>(directory + "/" + Path.GetFileNameWithoutExtension(newDirEntry));
                if (material == null)
                {
                    materialPreviews.Add(null);
                    continue;
                }
                Texture previewTexture = material.mainTexture;
                if (previewTexture == null)
                {
                    // color is a value type, so the color will never be null
                    Texture2D solidColorTexture = new Texture2D(128, 128);
                    for (int y = 0; y < solidColorTexture.height; y++)
                    {
                        for (int x = 0; x < solidColorTexture.height; x++)
                        {
                            solidColorTexture.SetPixel(x, y, material.color);
                        }
                    }
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
        string materialPath = materialDirectory + "/" + name;
        Material material = Resources.Load<Material>(materialPath);
        voxelArray.TestAssignMaterial(material);
        Destroy(this);
    }
}
