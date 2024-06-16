using System.Collections.Generic;
using UnityEngine;

// Remember that Unity resource paths always use forward slashes
public static class ResourcesDirectory
{
    public enum ColorStyle
    {
        TINT, PAINT
    }

    // map name to info
    private static Dictionary<string, MaterialInfo> _materialInfos = null;
    public static Dictionary<string, MaterialInfo> materialInfos
    {
        get
        {
            if (_materialInfos == null)
            {
                MaterialDatabase database = Resources.Load<MaterialDatabase>("materials");
                _materialInfos = new Dictionary<string, MaterialInfo>();
                foreach (MaterialInfo info in database.materials)
                {
                    _materialInfos.Add(info.name, info);
                }
            }
            return _materialInfos;
        }
    }

    private static ModelDatabase modelDatabase;

    public static ModelDatabase GetModelDatabase()
    {
        if (modelDatabase == null)
            modelDatabase = Resources.Load<ModelDatabase>("models");
        return modelDatabase;
    }

    public static Material LoadMaterial(MaterialInfo info) =>
        Resources.Load<Material>("GameAssets/" + info.path);

    public static Material FindMaterial(string name, bool editor)
    {
        // special alternate materials for game
        if ((!editor) && materialInfos.TryGetValue("$" + name, out MaterialInfo info))
            return LoadMaterial(info);
        if (materialInfos.TryGetValue(name, out info))
            return LoadMaterial(info);
        return null;
    }

    public static Material InstantiateMaterial(Material mat)
    {
        string name = mat.name;
        mat = Material.Instantiate(mat);
        mat.name = name;
        return mat;
    }

    public static string MaterialColorProperty(Material mat)
    {
        if (mat.HasProperty("_Color"))
            return "_Color";
        else if (mat.HasProperty("_Tint"))  // skybox
            return "_Tint";
        else if (mat.HasProperty("_SkyTint"))  // procedural skybox
            return "_SkyTint";
        else
            return null;
    }

    public static MaterialSound GetMaterialSound(Material material)
    {
        if (material == null)
            return MaterialSound.GENERIC;
        string name;
        if (CustomTexture.IsCustomTexture(material))
            name = CustomTexture.GetBaseMaterialName(material);
        else
            name = material.name;
        // special alternate materials for game
        if (materialInfos.TryGetValue("$" + name, out MaterialInfo info))
            return info.sound;
        if (materialInfos.TryGetValue(name, out info))
            return info.sound;
        return MaterialSound.GENERIC;
    }

    public static ColorStyle GetMaterialColorStyle(Material material)
    {
        if (!material.HasProperty("_MainTex"))
            return ColorStyle.PAINT;
        return material.mainTexture == null ? ColorStyle.PAINT : ColorStyle.TINT;
    }

    public static void SetMaterialColorStyle(Material material, ColorStyle style)
    {
        if (style == ColorStyle.PAINT)
            material.mainTexture = null;
        else if (style == ColorStyle.TINT)
            material.mainTexture = FindMaterial(material.name, true).mainTexture;
    }
}
