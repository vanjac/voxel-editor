using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class VoxelEditorGUI : MonoBehaviour {

    const float targetHeight = 360;

    public Rect guiRect;
    public float scaleFactor;

    public VoxelArray voxelArray;

    string[] dirList;

    string materialDirectory = "GameAssets/Materials";
    List<string> materials;
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

        // Make a background box
        GUI.Box(guiRect, "Assign Material");


        if (materials == null)
            return;
        Rect scrollBox = new Rect(guiRect.xMin, guiRect.yMin + 40, guiRect.width, guiRect.height - 40);
        Rect scrollArea = new Rect(0, 0, guiRect.width - 40, materials.Count * 30);
        matListScroll = GUI.BeginScrollView(scrollBox, matListScroll, scrollArea);
        for (int i = 0; i < materials.Count; i++)
        {
            if (GUI.Button(new Rect(10, 30 * i, scrollArea.width - 20, 20), materials[i]))
            {
                MaterialSelected(materials[i]);
            }
        }
        GUI.EndScrollView();
    }

    void UpdateMaterialDirectory()
    {
        Debug.Log(materialDirectory);
        materials = new List<string>();
        materials.Add("..");
        foreach (string dirEntry in dirList)
        {
            if (dirEntry.Length <= 2)
                continue;
            string newDirEntry = dirEntry.Substring(2);
            string directory = Path.GetDirectoryName(newDirEntry);
            string extension = Path.GetExtension(newDirEntry);
            if (directory == materialDirectory && (extension == ".mat" || extension == ""))
                materials.Add(Path.GetFileName(newDirEntry));
        }
    }

    void MaterialSelected(string name)
    {
        if (name == "..")
        {
            materialDirectory = Path.GetDirectoryName(materialDirectory);
            UpdateMaterialDirectory();
            return;
        }
        if (name.EndsWith(".mat"))
        {
            string materialPath = materialDirectory + "/" + Path.GetFileNameWithoutExtension(name);
            Material material = Resources.Load<Material>(materialPath);
            Debug.Log(material);
            voxelArray.TestAssignMaterial(material);
        }
        else
        {
            materialDirectory += "/" + name;
            UpdateMaterialDirectory();
        }
    }
}
