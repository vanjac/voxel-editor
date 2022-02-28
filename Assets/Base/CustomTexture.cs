using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// wraps a Material, for editing properties of a custom texture
public class CustomTexture : PropertiesObject
{
    public static PropertiesObjectType objectType = new PropertiesObjectType(
        "Custom Texture", "A custom texture material",
        "image", typeof(CustomTexture));

    public const string DEFAULT_CATEGORY = " CUSTOM "; // leading space for sorting order (sorry)
    private const string CUSTOM_NAME_PREFIX = "Custom:";
    private const string SHADER_NAME_PREFIX = "N-Space/";
    private static readonly string[] SHADER_NAMES = new string[] { "NDiffuse", "NUnlit" };
    private static readonly string[] TRANSPARENCY_NAMES = new string[] { "-Fade", "-Cutout" };
    private const string DOUBLE_SIDED_NAME = "-Double";

    public enum CustomShader
    {
        MATTE, UNLIT
    }

    public enum CustomTransparency
    {
        FADE, CUTOUT
    }

    public enum CustomFilter
    {
        SMOOTH, PIXEL
    }

    public Material material;
    public PaintLayer layer;
    public string category = DEFAULT_CATEGORY; // leading space for sorting order (sorry)

    protected CustomShader shader
    {
        get => material.shader.name.Contains(SHADER_NAMES[(int)CustomShader.UNLIT]) ?
            CustomShader.UNLIT : CustomShader.MATTE;
        set => material.shader = GetShader(value, transparency, doubleSided);
    }

    protected CustomTransparency transparency
    {
        get => material.shader.name.Contains(TRANSPARENCY_NAMES[(int)CustomTransparency.CUTOUT]) ?
            CustomTransparency.CUTOUT : CustomTransparency.FADE;
        set => material.shader = GetShader(shader, value, doubleSided);
    }

    protected bool doubleSided
    {
        get => material.shader.name.Contains(DOUBLE_SIDED_NAME);
        set => material.shader = GetShader(shader, transparency, value);
    }

    public Color color
    {
        get => material.color;
        set => material.color = value;
    }

    public Texture2D texture
    {
        get
        {
            // make sure value is never null!
            var tex = (Texture2D)material.mainTexture;
            if (tex == null)
                return Texture2D.whiteTexture;
            return tex;
        }
        set => material.mainTexture = value;
    }

    protected (float, float) scale
    {
        get
        {
            Vector2 s = material.mainTextureScale;
            return (s.x == 0 ? 0 : (1.0f / s.x), s.y == 0 ? 0 : (1.0f / s.y));
        }
        set
        {
            material.mainTextureScale = new Vector2(
                value.Item1 == 0 ? 0 : (1.0f / value.Item1), value.Item2 == 0 ? 0 : (1.0f / value.Item2));
        }
    }

    protected CustomFilter filter
    {
        get => texture.filterMode == FilterMode.Point ? CustomFilter.PIXEL : CustomFilter.SMOOTH;
        set => texture.filterMode = (value == CustomFilter.PIXEL ? FilterMode.Point : FilterMode.Bilinear);
    }

    protected MaterialSound sound
    {
        get => ResourcesDirectory.GetMaterialSound(material);
        set => material.SetInt("_Sound", (int)value);
    }

    public CustomTexture(PaintLayer layer)
    {
        this.layer = layer;
        material = new Material(GetShader(CustomShader.MATTE, CustomTransparency.FADE, false));
        material.name = CUSTOM_NAME_PREFIX + System.Guid.NewGuid();
    }

    public CustomTexture(Material material, PaintLayer layer)
    {
        this.layer = layer;
        this.material = material;
        this.material.name = CUSTOM_NAME_PREFIX + System.Guid.NewGuid();
    }

    public PropertiesObjectType ObjectType()
    {
        return objectType;
    }

    public ICollection<Property> Properties()
    {
        ICollection<Property> properties = new Property[]
        {
            new Property("cat", "Category",
                () => category,
                v => category = (string)v,
                PropertyGUIs.Text), // TODO menu of existing categories
            new Property("shd", "Shader",
                () => shader,
                v => shader = (CustomShader)v,
                PropertyGUIs.Enum),
        };
        if (layer == PaintLayer.OVERLAY)
        {
            properties = Property.JoinProperties(properties, new Property[]
            {
                new Property("tra", "Transparency",
                    () => transparency,
                    v => transparency = (CustomTransparency)v,
                    PropertyGUIs.Enum),
                new Property("dou", "Double sided?",
                    () => doubleSided,
                    v => doubleSided = (bool)v,
                    PropertyGUIs.Toggle),
            });
        }
        return Property.JoinProperties(properties, new Property[]
        {
            new Property("col", "Color",
                () => color,
                v => color = (Color)v,
                PropertyGUIs.Color),
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
                PropertyGUIs.Enum),
            new Property("sou", "Sound",
                () => sound,
                v => sound = (MaterialSound)v,
                PropertyGUIs.Enum),
        });
    }

    public ICollection<Property> DeprecatedProperties()
    {
        return new Property[]
        {
            new Property("bas", "Base",
                () => material,
                v =>
                {
                    Material baseMat = (Material)v;
                    if (baseMat != null)
                    {
                        CustomTexture copyFrom = new CustomTexture(baseMat, layer);
                        shader = copyFrom.shader;
                        transparency = copyFrom.transparency;
                        // don't copy double-sided, wasn't supported for old-style custom textures
                        color = copyFrom.color;
                    }
                },
                PropertyGUIs.Empty)
        };
    }

    private Shader GetShader(CustomShader shaderType, CustomTransparency transparency,
        bool doubleSided)
    {
        string shaderName = SHADER_NAME_PREFIX + SHADER_NAMES[(int)shaderType];
        if (layer == PaintLayer.OVERLAY)
        {
            shaderName += TRANSPARENCY_NAMES[(int)transparency];
            if (doubleSided)
                shaderName += DOUBLE_SIDED_NAME;
        }
        return Shader.Find(shaderName);
    }

    public static bool IsCustomTexture(Material material)
    {
        return material.name.StartsWith(CUSTOM_NAME_PREFIX);
    }

    public static bool IsSupportedShader(Material material)
    {
        return material.shader.name.StartsWith(SHADER_NAME_PREFIX);
    }
}
