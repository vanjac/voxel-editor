using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum MaterialType {
    None, Material, Overlay, Sky, Preview
}

public enum MaterialSound {
    GENERIC, CONCRETE, ROCK, PLASTER, FABRIC, DIRT, GRASS, GRAVEL, SAND, METAL,
    TILE, SNOW, ICE, WOOD, METAL_GRATE, GLASS, WATER, CHAIN_LINK, SWIM
}

public class MaterialInfo {
    public string name;
    public string gameMat;

    public MaterialType type = MaterialType.None;
    public string category = "";
    public MaterialSound sound = MaterialSound.GENERIC;
    public Color whitePoint = Color.white;
    public bool supportsColorStyles;
}

public struct ModelCategory {
    public Texture2D icon;
    public List<string> models;
}

// Remember that Unity resource paths always use forward slashes
public static class ResourcesDirectory {
    public enum ColorStyle {
        TINT, PAINT
    }

    private struct MaterialConfigState {
        public string category;
        public MaterialSound sound;
        public MaterialInfo material;
    }

    private struct ModelConfigState {
        public ModelCategory category;
    }

    private static List<MaterialInfo> materials;
    private static Dictionary<string, MaterialInfo> namedMaterials =
        new Dictionary<string, MaterialInfo>();
    private static Dictionary<string, MaterialInfo> namedPreviewMaterials =
        new Dictionary<string, MaterialInfo>();
    private static Dictionary<MaterialType, List<string>> materialCategories =
        new Dictionary<MaterialType, List<string>>();

    private static List<ModelCategory> modelCategories;

    private static void EnsureMaterialsLoaded() {
        if (materials == null) {
            materials = new List<MaterialInfo>();
            LoadMaterialType(MaterialType.None, "hiddenmat");
            LoadMaterialType(MaterialType.Material, "materials");
            LoadMaterialType(MaterialType.Overlay, "overlays");
            LoadMaterialType(MaterialType.Sky, "skies");
        }
    }

    private static void LoadMaterialType(MaterialType type, string fileName) {
        var categories = new List<string>();
        materialCategories[type] = categories;

        var script = Resources.Load<TextAsset>(fileName).text;
        var parser = new ConfigParser<MaterialConfigState>();
        parser.state.category = "";
        parser.Parse(new System.IO.StringReader(script), (cmd, args, l) => {
            if (cmd == "cat") {
                parser.state.category = args;
                if (args != "") {
                    categories.Add(args);
                }
            } else if (cmd == "sound") {
                if (Enum.TryParse<MaterialSound>(args, out var sound)) {
                    parser.state.sound = sound;
                } else {
                    throw new ConfigParser.ConfigException("Unrecognized sound", l);
                }
            } else if (cmd == "mat") {
                parser.state.material = new MaterialInfo() {
                    name = args,
                    type = type,
                    category = parser.state.category,
                    sound = parser.state.sound,
                    supportsColorStyles = true,
                };
                materials.Add(parser.state.material);
                namedMaterials.Add(args, parser.state.material);
            } else if (cmd == "nopaint") {
                parser.state.material.supportsColorStyles = false;
            } else if (cmd == "white") {
                var words = ConfigParser.SplitWords(args);
                var values = words.Select(s => ConfigParser.ParseFloat(s)).ToArray();
                parser.state.material.whitePoint = new Color(values[0], values[1], values[2]);
            } else if (cmd == "preview") {
                // TODO!
                var previewMat = new MaterialInfo() {
                    name = parser.state.material.name,
                    type = MaterialType.Preview,
                    category = parser.state.category,
                    supportsColorStyles = parser.state.material.supportsColorStyles,
                };
                materials.Add(previewMat);
                namedPreviewMaterials.Add(previewMat.name, previewMat);
            } else if (cmd == "ingame") {
                parser.state.material.gameMat = args;
            }
        });
    }

    public static List<MaterialInfo> GetMaterials() {
        EnsureMaterialsLoaded();
        return materials;
    }

    public static List<string> GetMaterialCategories(MaterialType type) {
        EnsureMaterialsLoaded();
        return materialCategories[type];
    }

    public static List<ModelCategory> GetModelCategories() {
        if (modelCategories == null) {
            modelCategories = LoadModelCategories();
        }
        return modelCategories;
    }

    private static List<ModelCategory> LoadModelCategories() {
        var categories = new List<ModelCategory>();

        var script = Resources.Load<TextAsset>("models").text;
        var parser = new ConfigParser<ModelConfigState>();
        parser.Parse(new System.IO.StringReader(script), (cmd, args, l) => {
            if (cmd == "cat") {
                parser.state.category = new ModelCategory() {
                    icon = Resources.Load<Texture2D>("Icons/" + args),
                    models = new List<string>()
                };
                categories.Add(parser.state.category);
            } else if (cmd == "mdl") {
                parser.state.category.models.Add(args);
            }
        });
        return categories;
    }

    private static string MaterialDirectory(MaterialType type) =>
        "GameAssets/" + type switch {
            MaterialType.None => "Hidden/",
            MaterialType.Material => "Materials/",
            MaterialType.Overlay => "Overlays/",
            MaterialType.Sky => "Skies/",
            MaterialType.Preview => "Previews/",
            _ => "",
        };

    public static Material LoadMaterial(MaterialInfo info, bool editor) {
        var name = (!editor && info.gameMat != null) ? info.gameMat : info.name;
        return Resources.Load<Material>(MaterialDirectory(info.type) + name);
    }

    public static bool FindMaterialInfo(string name, out MaterialInfo info) {
        EnsureMaterialsLoaded();
        return namedMaterials.TryGetValue(name, out info);
    }

    public static Material FindMaterial(string name, bool editor) {
        if (FindMaterialInfo(name, out var info)) {
            return LoadMaterial(info, editor);
        }
        return null;
    }

    public static Material FindPreviewMaterial(string name) {
        EnsureMaterialsLoaded();
        if (namedPreviewMaterials.TryGetValue(name, out var info)) {
            return LoadMaterial(info, true);
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
        if (FindMaterialInfo(name, out var info)) {
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
