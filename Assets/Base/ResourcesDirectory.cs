using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class ResourcesDirectory
{
    private static string[] _dirList = null;
    public static string[] dirList
    {
        get
        {
            if (_dirList == null)
            {
                TextAsset dirListText = Resources.Load<TextAsset>("dirlist");
                _dirList = dirListText.text.Split('\n');
                // fix issue when checking out a branch in git:
                for (int i = 0; i < _dirList.Length; i++)
                    _dirList[i] = _dirList[i].Trim();
                Resources.UnloadAsset(dirListText);
            }
            return _dirList;
        }
    }

    public static Material GetMaterial(string path)
    {
        // remove extension if necessary
        path = Path.GetDirectoryName(path) + "/" + Path.GetFileNameWithoutExtension(path);
        return Resources.Load<Material>(path);
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
