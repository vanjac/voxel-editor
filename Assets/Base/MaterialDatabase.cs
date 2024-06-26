using System.Collections.Generic;
using UnityEngine;

public enum MaterialSound
{
    GENERIC, CONCRETE, ROCK, PLASTER, FABRIC, DIRT, GRASS, GRAVEL, SAND, METAL,
    TILE, SNOW, ICE, WOOD, METAL_GRATE, GLASS, WATER, CHAIN_LINK, SWIM
}

// could represent a material or a directory!
[System.Serializable]
public struct MaterialInfo
{
    public bool isDirectory;
    public string name;
    // without extension, starting from Assets/Resources/GameAssets/
    public string path;
    public string parent; // parent directory
    public MaterialSound sound;
    public Color whitePoint;
    public bool supportsColorStyles;
}

[System.Serializable]
[CreateAssetMenu(fileName = "materials", menuName = "ScriptableObjects/N-Space Material Database")]
public class MaterialDatabase : ScriptableObject
{
    public List<MaterialInfo> materials = new List<MaterialInfo>();
}