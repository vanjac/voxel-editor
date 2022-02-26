﻿using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

public enum MaterialSound
{
    GENERIC, CONCRETE, ROCK, PLASTER, FABRIC, DIRT, GRASS, GRAVEL, SAND, METAL,
    TILE, SNOW, ICE, WOOD, METAL_GRATE, GLASS, WATER, CHAIN_LINK, SWIM
}

// Remember that Unity resource paths always use forward slashes
public static class ResourcesDirectory
{
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

    public static Material LoadMaterial(MaterialInfo info, bool editor)
    {
        string path = (!editor && info.gamePath != "") ? info.gamePath : info.path;
        return Resources.Load<Material>("GameAssets/" + path);
    }

    public static Material FindMaterial(string name, bool editor)
    {
        MaterialInfo info;
        if (materialInfos.TryGetValue(name, out info))
            return LoadMaterial(info, editor);
        return null;
    }

    public static Material InstantiateMaterial(Material mat)
    {
        string name = mat.name;
        mat = Material.Instantiate(mat);
        mat.name = name;
        return mat;
    }

    public static MaterialSound GetMaterialSound(Material material)
    {
        if (material == null)
            return MaterialSound.GENERIC;
        if (material.HasProperty("_Sound"))
            return (MaterialSound)material.GetInt("_Sound");
        return MaterialSound.GENERIC;
    }
}
