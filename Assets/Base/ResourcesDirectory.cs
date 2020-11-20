using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Rendering;

// Remember that Unity resource paths always use forward slashes
public class ResourcesDirectory
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

    public static Material LoadMaterial(MaterialInfo info)
    {
        return Resources.Load<Material>("GameAssets/" + info.path);
    }

    public static Material FindMaterial(string name, bool editor)
    {
        MaterialInfo info;
        // special alternate materials for game
        if ((!editor) && materialInfos.TryGetValue("$" + name, out info))
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
        MaterialInfo info;
        // special alternate materials for game
        if (materialInfos.TryGetValue("$" + material.name, out info))
            return info.sound;
        if (materialInfos.TryGetValue(material.name, out info))
            return info.sound;
        return MaterialSound.GENERIC;
    }
}
