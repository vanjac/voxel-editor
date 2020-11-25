using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// wraps a Material, for editing properties of a custom texture
public struct CustomTexture
{
    public Material material;

    public string baseName
    {
        get
        {
            return material.name.Split(':')[1];
        }
    }

    public Texture2D texture
    {
        get
        {
            return (Texture2D)material.mainTexture;
        }
        set
        {
            material.mainTexture = value;
        }
    }

    public Vector2 scale
    {
        get
        {
            return material.mainTextureScale;
        }
        set
        {
            material.mainTextureScale = value;
        }
    }

    public CustomTexture(Material material)
    {
        this.material = material;
    }

    public static CustomTexture FromBaseMaterial(Material baseMat)
    {
        Material material = Material.Instantiate(baseMat);
        material.name = "Custom:" + baseMat.name + ":" + System.Guid.NewGuid();
        return new CustomTexture(material);
    }

    public static bool IsCustomTexture(string name)
    {
        return name.StartsWith("Custom:");
    }
}
