using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
#if UNITY_WEBGL
using UnityEngine.Networking;
#endif

// https://docs.unity3d.com/Manual/BuiltInImporters.html
// https://unity.com/blog/engine-platform/unity-asset-bundles-tips-pitfalls

public enum MaterialType {
    None, Material, Overlay, Sky
}

public enum MaterialSound {
    GENERIC, CONCRETE, ROCK, PLASTER, FABRIC, DIRT, GRASS, GRAVEL, SAND, METAL,
    TILE, SNOW, ICE, WOOD, METAL_GRATE, GLASS, WATER, CHAIN_LINK, SWIM
}

public class MaterialInfo {
    public string name;
    public string gameMat;
    public string previewMat;

    public MaterialType type = MaterialType.None;
    public string category = "";
    public MaterialSound sound = MaterialSound.GENERIC;
    public Color whitePoint = Color.white;
    public bool supportsColorStyles;
}

public class ModelCategory {
    public Texture2D icon;
    public List<string> models = new List<string>();
}

public class MaterialSoundData {
    public float volume = 1;
    public List<AudioClip> left = new List<AudioClip>();
    public List<AudioClip> right = new List<AudioClip>();
}

public class AssetPack {
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

    private struct MaterialSoundConfigState {
        public MaterialSoundData data;
    }

    private static AssetPack current;

    private AssetBundle assetBundle;
    private List<MaterialInfo> materials;
    private Dictionary<string, MaterialInfo> namedMaterials =
        new Dictionary<string, MaterialInfo>();
    private Dictionary<MaterialType, List<string>> materialCategories =
        new Dictionary<MaterialType, List<string>>();
    private Dictionary<MaterialSound, MaterialSoundData> materialSounds;

    private List<ModelCategory> modelCategories;

    private static string GetPath() {
        var platformName = Application.platform.ToString();
        platformName = Regex.Replace(platformName, "(Player|Editor)", "").ToLower();
        var bundleName = "nspace_default_" + platformName;
        return System.IO.Path.Combine(Application.streamingAssetsPath, bundleName);
    }

    public static IEnumerator LoadAsync() {
        if (current != null) {
            yield break;
        }
#if UNITY_WEBGL
        var request = UnityWebRequestAssetBundle.GetAssetBundle(GetPath());
        Debug.Log("Downloading AssetBundle...");
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success) {
            Debug.LogError(request.error);
        } else {
            Debug.Log("Download complete");
            var bundle = DownloadHandlerAssetBundle.GetContent(request);
            if (bundle) {
                current = new AssetPack();
            }
        }
#else
        Current(); // TODO: AssetBundle.LoadFromFileAsync();
        yield break;
#endif
    }

    public static AssetPack Current() {
        if (current == null) {
            var bundle = AssetBundle.LoadFromFile(GetPath());
            if (bundle) {
                current = new AssetPack(bundle);
            }
        }
        return current;
    }

    // slow and dangerous!
    public static void UnloadUnused() {
        Resources.UnloadUnusedAssets();
    }

    public AssetPack(AssetBundle bundle) {
        assetBundle = bundle;
    }

    public string LoadConfigFile(string name) {
        var asset = assetBundle.LoadAsset<TextAsset>("Config/" + name);
        return asset ? asset.text : "";
    }

    private void EnsureMaterialsLoaded() {
        if (materials == null) {
            materials = new List<MaterialInfo>();
            LoadMaterialType(MaterialType.None, "hiddenmat");
            LoadMaterialType(MaterialType.Material, "materials");
            LoadMaterialType(MaterialType.Overlay, "overlays");
            LoadMaterialType(MaterialType.Sky, "skies");
        }
    }

    private void LoadMaterialType(MaterialType type, string fileName) {
        var categories = new List<string>();
        materialCategories[type] = categories;

        var script = LoadConfigFile(fileName);
        var parser = new ConfigParser<MaterialConfigState>();
        parser.state.category = "";
        parser.Parse(new System.IO.StringReader(script), (cmd, args, l) => {
            if (cmd == "cat") {
                // TODO: localize category names
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
                parser.state.material.previewMat = args;
            } else if (cmd == "ingame") {
                parser.state.material.gameMat = args;
            }
        });
    }

    public List<MaterialInfo> GetMaterials() {
        EnsureMaterialsLoaded();
        return materials;
    }

    public List<string> GetMaterialCategories(MaterialType type) {
        EnsureMaterialsLoaded();
        return materialCategories[type];
    }

    public List<ModelCategory> GetModelCategories() {
        if (modelCategories == null) {
            modelCategories = LoadModelCategories();
        }
        return modelCategories;
    }

    private List<ModelCategory> LoadModelCategories() {
        var categories = new List<ModelCategory>();

        var script = LoadConfigFile("models");
        var parser = new ConfigParser<ModelConfigState>();
        parser.Parse(new System.IO.StringReader(script), (cmd, args, l) => {
            if (cmd == "cat") {
                parser.state.category = new ModelCategory() {
                    icon = assetBundle.LoadAsset<Texture2D>("Icons/" + args),
                };
                categories.Add(parser.state.category);
            } else if (cmd == "mdl") {
                parser.state.category.models.Add(args);
            }
        });
        return categories;
    }

    public void EnsureMaterialSoundsLoaded() {
        if (materialSounds == null) {
            materialSounds = LoadMaterialSounds();
        }
    }

    private Dictionary<MaterialSound, MaterialSoundData> LoadMaterialSounds() {
        var materialSounds = new Dictionary<MaterialSound, MaterialSoundData>();

        var script = LoadConfigFile("matsounds");
        var parser = new ConfigParser<MaterialSoundConfigState>();
        parser.Parse(new System.IO.StringReader(script), (cmd, args, l) => {
            if (cmd == "sound") {
                if (Enum.TryParse<MaterialSound>(args, out var sound)) {
                    parser.state.data = new MaterialSoundData();
                    materialSounds.Add(sound, parser.state.data);
                } else {
                    throw new ConfigParser.ConfigException("Unrecognized sound", l);
                }
            } else if (cmd == "left") {
                parser.state.data.left.Add(LoadSound(args));
            } else if (cmd == "right") {
                parser.state.data.right.Add(LoadSound(args));
            } else if (cmd == "volume") {
                parser.state.data.volume = ConfigParser.ParseFloat(args);
            }
        });
        return materialSounds;
    }

    public Material LoadMaterial(MaterialInfo info, bool editor) {
        var name = (!editor && info.gameMat != null) ? info.gameMat : info.name;
        return assetBundle.LoadAsset<Material>("Materials/" + name);
    }

    public Material LoadMaterialPreview(MaterialInfo info) {
        if (info.previewMat != null) {
            return assetBundle.LoadAsset<Material>("Previews/" + info.previewMat);
        }
        return LoadMaterial(info, true);
    }

    public bool FindMaterialInfo(string name, out MaterialInfo info) {
        EnsureMaterialsLoaded();
        return namedMaterials.TryGetValue(name, out info);
    }

    public Material FindMaterial(string name, bool editor) {
        if (FindMaterialInfo(name, out var info)) {
            return LoadMaterial(info, editor);
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

    public MaterialSound GetMaterialSound(Material material) {
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

    public MaterialSound GetMaterialSound(VoxelFace face) {
        MaterialSound matSound = GetMaterialSound(face.material);
        MaterialSound overSound = GetMaterialSound(face.overlay);
        if (overSound == MaterialSound.GENERIC) {
            return matSound;
        } else {
            return overSound;
        }
    }

    public static ColorStyle GetMaterialColorStyle(Material material) {
        if (!material.HasProperty("_MainTex")) {
            return ColorStyle.PAINT;
        }
        return material.mainTexture == null ? ColorStyle.PAINT : ColorStyle.TINT;
    }

    public void SetMaterialColorStyle(Material material, ColorStyle style) {
        if (style == ColorStyle.PAINT) {
            material.mainTexture = null;
        } else if (style == ColorStyle.TINT) {
            material.mainTexture = FindMaterial(material.name, true).mainTexture;
        }
    }

    public Mesh LoadModel(string name) => assetBundle.LoadAsset<Mesh>("Models/" + name);

    public Texture2D GetModelThumbnail(string name) =>
        assetBundle.LoadAsset<Texture2D>("Thumbnails/" + name);

    private AudioClip LoadSound(string name) => assetBundle.LoadAsset<AudioClip>("Sounds/" + name);

    public MaterialSoundData GetMaterialSoundData(MaterialSound sound) {
        EnsureMaterialSoundsLoaded();
        if (materialSounds.TryGetValue(sound, out var data)) {
            return data;
        } else {
            return materialSounds[MaterialSound.GENERIC];
        }
    }
}
