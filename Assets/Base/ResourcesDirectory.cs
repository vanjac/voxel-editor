using System.Collections.Generic;
using UnityEngine;

// Remember that Unity resource paths always use forward slashes
public static class ResourcesDirectory {
    public enum ColorStyle {
        TINT, PAINT
    }

    private static MaterialDatabase materialDatabase;

    // map name to info
    private static Dictionary<string, MaterialInfo> materialInfos = null;

    private static Dictionary<MaterialType, List<string>> materialCategories =
        new Dictionary<MaterialType, List<string>>();

    private static ModelDatabase modelDatabase;

    public static MaterialDatabase GetMaterialDatabase() {
        if (materialDatabase == null) {
            materialDatabase = Resources.Load<MaterialDatabase>("materials");
        }
        return materialDatabase;
    }

    public static Dictionary<string, MaterialInfo> GetMaterialInfos() {
        if (materialInfos == null) {
            materialInfos = new Dictionary<string, MaterialInfo>();
            foreach (MaterialInfo info in GetMaterialDatabase().materials) {
                materialInfos.Add(info.name, info);
            }
        }
        return materialInfos;
    }

    public static List<string> GetMaterialCategories(MaterialType type) {
        if (materialCategories.TryGetValue(type, out var categories)) {
            return categories;
        }

        var categorySet = new SortedSet<string>();
        foreach (MaterialInfo matInfo in GetMaterialDatabase().materials) {
            if (matInfo.type == type && matInfo.category != "") {
                categorySet.Add(matInfo.category);
            }
        }
        categories = new List<string>(categorySet);
        materialCategories[type] = categories;
        return categories;
    }

    public static ModelDatabase GetModelDatabase() {
        if (modelDatabase == null) {
            modelDatabase = Resources.Load<ModelDatabase>("models");
        }
        return modelDatabase;
    }

    private static string MaterialDirectory(MaterialType type) =>
        "GameAssets/" + type switch {
            MaterialType.None => "Hidden/",
            MaterialType.Material => "Materials/",
            MaterialType.Overlay => "Overlays/",
            MaterialType.Sky => "Skies/",
            _ => "",
        };

    public static Material LoadMaterial(MaterialInfo info) =>
        Resources.Load<Material>(MaterialDirectory(info.type) + info.name);

    public static Material FindMaterial(string name, bool editor) {
        // special alternate materials for game
        var infos = GetMaterialInfos();
        if ((!editor) && infos.TryGetValue("$" + name, out MaterialInfo info)) {
            return LoadMaterial(info);
        }
        if (infos.TryGetValue(name, out info)) {
            return LoadMaterial(info);
        }
        return null;
    }

    public static Material InstantiateMaterial(Material mat) {
        string name = mat.name;
        mat = Material.Instantiate(mat);
        mat.name = name;
        return mat;
    }

    public static string MaterialColorProperty(Material mat) {
        if (mat.HasProperty("_Color")) {
            return "_Color";
        } else if (mat.HasProperty("_Tint")) { // skybox
            return "_Tint";
        } else if (mat.HasProperty("_SkyTint")) { // procedural skybox
            return "_SkyTint";
        } else {
            return null;
        }
    }

    public static MaterialSound GetMaterialSound(Material material) {
        if (material == null) {
            return MaterialSound.GENERIC;
        }
        string name;
        if (CustomTexture.IsCustomTexture(material)) {
            name = CustomTexture.GetBaseMaterialName(material);
        } else {
            name = material.name;
        }
        if (GetMaterialInfos().TryGetValue(name, out var info)) {
            return info.sound;
        }
        return MaterialSound.GENERIC;
    }

    public static ColorStyle GetMaterialColorStyle(Material material) {
        if (!material.HasProperty("_MainTex")) {
            return ColorStyle.PAINT;
        }
        return material.mainTexture == null ? ColorStyle.PAINT : ColorStyle.TINT;
    }

    public static void SetMaterialColorStyle(Material material, ColorStyle style) {
        if (style == ColorStyle.PAINT) {
            material.mainTexture = null;
        } else if (style == ColorStyle.TINT) {
            material.mainTexture = FindMaterial(material.name, true).mainTexture;
        }
    }

    public static Texture2D GetModelThumbnail(string name) =>
        Resources.Load<Texture2D>("Thumbnails/" + name);
}
