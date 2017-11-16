﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class VoxelEditorGUI : MonoBehaviour {

    const float targetHeight = 360;

    public Rect guiRect;
    public float scaleFactor;

    public VoxelArray voxelArray;
    public Transform cameraPivot;

    string[] dirList;

    List<string> materialNames;
    List<Texture> materialPreviews;
    string materialDirectory = "GameAssets/Materials";
    List<string> materialSubDirectories;
    Vector2 matListScroll;

    void Start()
    {
        TextAsset dirListText = Resources.Load<TextAsset>("dirlist");
        dirList = dirListText.text.Split('\n');
        UpdateMaterialDirectory();
    }

    void OnGUI()
    {
        scaleFactor = Screen.height / targetHeight;
        GUI.matrix = Matrix4x4.Scale(new Vector3(scaleFactor, scaleFactor, 1));

        guiRect = new Rect(10, 10, 180, targetHeight - 20);

        if (GUI.Button(new Rect(guiRect.xMax + 10, 10, 80, 20), "Save"))
        {
            MapFileWriter writer = new MapFileWriter("mapsave");
            writer.Write(cameraPivot, voxelArray);
        }

        if (GUI.Button(new Rect(guiRect.xMax + 100, 10, 80, 20), "Load"))
        {
        }

        // Make a background box
        GUI.Box(guiRect, "Assign Material");


        if (materialPreviews == null)
            return;
        Rect scrollBox = new Rect(guiRect.xMin, guiRect.yMin + 40, guiRect.width, guiRect.height - 40);
        float scrollAreaWidth = guiRect.width - 40;
        float scrollAreaHeight = materialSubDirectories.Count * 25 + materialPreviews.Count * (scrollAreaWidth - 20);
        Rect scrollArea = new Rect(0, 0, scrollAreaWidth, scrollAreaHeight);
        matListScroll = GUI.BeginScrollView(scrollBox, matListScroll, scrollArea);
        float y = 0;
        for (int i = 0; i < materialSubDirectories.Count; i++)
        {
            string subDir = materialSubDirectories[i];
            if (GUI.Button(new Rect(10, y, scrollArea.width - 20, 20), subDir))
            {
                MaterialDirectorySelected(materialSubDirectories[i]);
            }
            y += 25;
        }
        for (int i = 0; i < materialPreviews.Count; i++)
        {
            Texture materialPreview = materialPreviews[i];
            if (GUI.Button(new Rect(10, y, scrollArea.width - 20, scrollArea.width - 20), materialPreview))
            {
                MaterialSelected(materialNames[i]);
            }
            y += scrollArea.width - 20;
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
        foreach (string dirEntry in dirList)
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
            materialDirectory = Path.GetDirectoryName(materialDirectory);
            UpdateMaterialDirectory();
            return;
        }
        else
        {
            materialDirectory += "/" + name;
            UpdateMaterialDirectory();
        }
    }

    void MaterialSelected(string name)
    {
        string materialPath = materialDirectory + "/" + name;
        Material material = Resources.Load<Material>(materialPath);
        voxelArray.TestAssignMaterial(material);
    }
}
