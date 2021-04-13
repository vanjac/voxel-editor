using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// wraps a Material, for editing properties of a custom texture
public class CustomTexture : PropertiesObject
{
    public static PropertiesObjectType objectType = new PropertiesObjectType(
        "Custom Texture", "A custom texture image for materials or overlays",
        "image", typeof(CustomTexture));

    public enum CustomFilter
    {
        SMOOTH, PIXEL
    }

    private Material _material, _baseMat;
    private bool isOverlay;

    public Material material
    {
        get
        {
            return _material;
        }
    }

    public Texture2D texture
    {
        get
        {
            // make sure value is never null!
            var tex = (Texture2D)_material.mainTexture;
            if (tex == null)
                return Texture2D.whiteTexture;
            return tex;
        }
        set
        {
            _material.mainTexture = value;
        }
    }

    protected (float, float) scale
    {
        get
        {
            Vector2 s = _material.mainTextureScale;
            return (s.x == 0 ? 0 : (1.0f / s.x), s.y == 0 ? 0 : (1.0f / s.y));
        }
        set
        {
            _material.mainTextureScale = new Vector2(
                value.Item1 == 0 ? 0 : (1.0f / value.Item1), value.Item2 == 0 ? 0 : (1.0f / value.Item2));
        }
    }

    public Material baseMat
    {
        get
        {
            return _baseMat;
        }
        set
        {
            _baseMat = value;
            if (_material == null)
            {
                _material = new Material(_baseMat);
            }
            else
            {
                Texture2D oldTexture = texture;
                (float, float) oldScale = scale;

                _material.shader = value.shader;
                _material.CopyPropertiesFromMaterial(value);

                texture = oldTexture;
                scale = oldScale;
            }

            _material.name = "Custom:" + _baseMat.name + ":" + System.Guid.NewGuid();
        }
    }

    protected CustomFilter filter
    {
        get
        {
            return texture.filterMode == FilterMode.Point ? CustomFilter.PIXEL : CustomFilter.SMOOTH;
        }
        set
        {
            texture.filterMode = (value == CustomFilter.PIXEL ? FilterMode.Point : FilterMode.Bilinear);
        }
    }

    public CustomTexture(Material material, bool isOverlay)
    {
        _material = material;
        this.isOverlay = isOverlay;
        if (_material != null && IsCustomTexture(_material))
        {
            string baseName = GetBaseMaterialName(_material);
            _baseMat = ResourcesDirectory.FindMaterial(baseName, true);
            string colorProp = ResourcesDirectory.MaterialColorProperty(_baseMat);
            // copied from MessagePackWorldReader
            Color color = _material.GetColor(colorProp);
            if (color != _baseMat.GetColor(colorProp))
            {
                _baseMat = ResourcesDirectory.InstantiateMaterial(_baseMat);
                _baseMat.SetColor(colorProp, color);
            }
        }
        else
            _baseMat = _material;  // probably temporary
    }

    public static CustomTexture FromBaseMaterial(Material baseMat, bool overlay)
    {
        var customTex = new CustomTexture(null, overlay);
        customTex.baseMat = baseMat;
        return customTex;
    }

    public override PropertiesObjectType ObjectType()
    {
        return objectType;
    }

    public override IEnumerable<Property> Properties()
    {
        return new Property[]
        {
            new Property("bas", "Base",
                () => baseMat,
                v => baseMat = (Material)v,
                PropertyGUIs.Material(isOverlay ? "Overlays" : "Materials", isOverlay, customTextureBase: true)),
            new Property("tex", "Texture",
                () => texture,
                v => texture = (Texture2D)v,
                PropertyGUIs.Texture),
            new Property("dim", "Size",
                () => scale,
                v => scale = ((float, float))v,
                PropertyGUIs.FloatDimensions),
            new Property("flt", "Filter",
                () => filter,
                v => filter = (CustomFilter)v,
                PropertyGUIs.Enum)
        };
    }

    public static bool IsCustomTexture(Material material)
    {
        return material.name.StartsWith("Custom:");
    }

    public static string GetBaseMaterialName(Material material)
    {
        return material.name.Split(':')[1];
    }

    public static Material Clone(Material material)
    {
        Material newMat = Material.Instantiate(material);
        Debug.Log("old name: " + material.name);
        var nameParts = material.name.Split(':');
        if (nameParts.Length < 3)
        {
            Debug.LogError("Bad material name!");
            return newMat;
        }
        newMat.name = nameParts[0] + ":" + nameParts[1] + ":" + System.Guid.NewGuid();
        Debug.Log("new name: " + newMat.name);
        return newMat;
    }
}
