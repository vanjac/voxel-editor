using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// could represent a material or a directory!
[System.Serializable]
public struct MaterialInfo {
    public bool isDirectory;
    public string name;
    // without extension, starting from Assets/Resources/GameAssets/
    public string path;
    public string parent; // parent directory
}

[System.Serializable]
public class MaterialDatabase : ScriptableObject
{
    public List<MaterialInfo> materials = new List<MaterialInfo>();
}