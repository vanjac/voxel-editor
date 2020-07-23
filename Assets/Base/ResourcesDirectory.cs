using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

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

    public static Material MakeCustomMaterial(ColorMode colorMode, bool transparent = false)
    {
        Material material = null;
        switch (colorMode)
        {
            case ColorMode.MATTE:
                material = new Material(Shader.Find("Standard"));
                material.SetFloat("_Glossiness", 0.0f);
                material.SetFloat("_Metallic", 0.0f);
                break;
            case ColorMode.GLOSSY:
                material = new Material(Shader.Find("Standard"));
                material.SetFloat("_Glossiness", 0.9f);
                material.SetFloat("_Metallic", 0.0f);
                break;
            case ColorMode.METAL:
                material = new Material(Shader.Find("Standard"));
                material.SetFloat("_Glossiness", 0.95f);
                material.SetFloat("_Metallic", 1.0f);
                break;
            case ColorMode.UNLIT:
                if (transparent)
                    material = new Material(Shader.Find("Unlit/UnlitColorTransparent"));
                else
                    material = new Material(Shader.Find("Unlit/Color"));
                break;
            case ColorMode.GLASS:
                material = new Material(Shader.Find("Standard"));
                material.SetFloat("_Glossiness", 0.973f);
                material.SetFloat("_Metallic", 0.273f);
                break;
            case ColorMode.ADD:
                material = new Material(Shader.Find("Unlit/UnlitAdd"));
                break;
            case ColorMode.MULTIPLY:
                material = new Material(Shader.Find("Unlit/UnlitMultiply"));
                break;
        }
        material.name = "Custom:" + colorMode.ToString() + ":" + System.Guid.NewGuid();

        if (transparent)
        {
            material.renderQueue = 3000;
            if (material.shader.name == "Standard")
            {
                material.SetFloat("_Mode", 3); // transparent
                // http://answers.unity.com/answers/1265884/view.html
                material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
                material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                material.SetInt("_ZWrite", 0);
                material.DisableKeyword("_ALPHATEST_ON");
                material.DisableKeyword("_ALPHABLEND_ON");
                material.EnableKeyword("_ALPHAPREMULTIPLY_ON");
            }
        }
        return material;
    }

    public static bool IsCustomMaterial(Material material)
    {
        return material.name.StartsWith("Custom");
    }

    public static ColorMode GetCustomMaterialColorMode(Material material)
    {
        string name = material.name.Split(':')[1];
        return (ColorMode)System.Enum.Parse(typeof(ColorMode), name);
    }

    public static bool GetCustomMaterialIsTransparent(Material material)
    {
        return material.renderQueue > 2000;
    }
}
