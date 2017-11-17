using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class VoxelEditorGUI : MonoBehaviour {

    const float targetHeight = 360;

    public Rect guiRect;
    public float scaleFactor;

    public VoxelArray voxelArray;
    public Transform cameraPivot;
    public GUISkin guiSkin;
    public Vector2 propertiesScroll;

    List<string> materialNames;
    List<Texture> materialPreviews;
    string materialDirectory = "GameAssets/Materials";
    List<string> materialSubDirectories;

    void Start()
    {
        UpdateMaterialDirectory();
    }

    void OnGUI()
    {
        GUI.skin = guiSkin;

        scaleFactor = Screen.height / targetHeight;
        GUI.matrix = Matrix4x4.Scale(new Vector3(scaleFactor, scaleFactor, 1));

        guiRect = new Rect(0, 0, 180, targetHeight);

        if (GUI.Button(new Rect(guiRect.xMax + 10, 10, 80, 20), "Save"))
        {
            MapFileWriter writer = new MapFileWriter("mapsave");
            writer.Write(cameraPivot, voxelArray);
        }

        if (GUI.Button(new Rect(guiRect.xMax + 100, 10, 80, 20), "Load"))
        {
            MapFileReader reader = new MapFileReader("mapsave");
            reader.Read(cameraPivot, voxelArray);
        }

        // Make a background box
        GUI.Box(guiRect, "Assign Material");


        if (materialPreviews == null)
            return;
        Rect scrollBox = new Rect(guiRect.xMin, guiRect.yMin + 25, guiRect.width, guiRect.height - 25);
        float scrollAreaWidth = guiRect.width - 5;
        float buttonWidth = scrollAreaWidth - 20;
        float scrollAreaHeight = materialSubDirectories.Count * 25 + materialPreviews.Count * buttonWidth;
        Rect scrollArea = new Rect(0, 0, scrollAreaWidth, scrollAreaHeight);
        propertiesScroll = GUI.BeginScrollView(scrollBox, propertiesScroll, scrollArea);
        float y = 0;
        for (int i = 0; i < materialSubDirectories.Count; i++)
        {
            string subDir = materialSubDirectories[i];
            if (GUI.Button(new Rect(10, y, buttonWidth, 20), subDir))
            {
                propertiesScroll = new Vector2(0, 0);
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
            if(materialDirectory.Trim() != "")
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
    }
}
