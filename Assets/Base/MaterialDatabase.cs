using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PaintLayer
{
    MATERIAL, OVERLAY, SKY, HIDDEN
}

// could represent a material or a directory!
[System.Serializable]
public struct MaterialInfo
{
    public string name;
    // without extension, starting from Assets/Resources/GameAssets/
    public string path;
    public string category;
    public PaintLayer layer;
    public Texture2D thumbnail;
}

[System.Serializable]
[CreateAssetMenu(fileName = "materials", menuName = "ScriptableObjects/N-Space Material Database")]
public class MaterialDatabase : ScriptableObject
{
    public List<MaterialInfo> materials = new List<MaterialInfo>();
}