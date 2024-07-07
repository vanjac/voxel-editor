using System.Collections.Generic;
using UnityEngine;

public enum MaterialType {
    None, Material, Overlay, Sky, Preview
}

public enum MaterialSound {
    GENERIC, CONCRETE, ROCK, PLASTER, FABRIC, DIRT, GRASS, GRAVEL, SAND, METAL,
    TILE, SNOW, ICE, WOOD, METAL_GRATE, GLASS, WATER, CHAIN_LINK, SWIM
}

[System.Serializable]
public struct MaterialInfo {
    public string name;
    public MaterialType type;
    public string category;
    public MaterialSound sound;
    public Color whitePoint;
    public bool supportsColorStyles;
}

[System.Serializable]
[CreateAssetMenu(fileName = "materials", menuName = "ScriptableObjects/N-Space Material Database")]
public class MaterialDatabase : ScriptableObject {
    public List<MaterialInfo> materials = new List<MaterialInfo>();
}