using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldProperties : PropertiesObject
{
    private static readonly Dictionary<string, float> skyRotations = new Dictionary<string, float>
    {
        {"sky5X1", 102},
        {"sky5X2", 143},
        {"sky5X3", 7},
        {"sky5X4", 177},
        {"sky5X5", 196}
    };

    public static PropertiesObjectType objectType = new PropertiesObjectType(
        "World", "Properties that affect the entire world", "earth", typeof(WorldProperties));

    [MaterialProp("sky", "Sky", "Skies", false)]
    public Material skybox
    {
        get => RenderSettings.skybox;
        set => SetSky(value);
    }
    [SliderProp("amb", "Ambient light intensity", 0, 3)]
    public float ambientIntensity
    {
        get => RenderSettings.ambientIntensity;
        set => RenderSettings.ambientIntensity = value;
    }
    [SliderProp("sin", "Sun intensity", 0, 3)]
    public float sunIntensity
    {
        get => RenderSettings.sun.intensity;
        set => RenderSettings.sun.intensity = value;
    }
    [ColorProp("sco", "Sun color")]
    public Color sunColor
    {
        get => RenderSettings.sun.color;
        set => RenderSettings.sun.color = value;
    }
    [SliderProp("spi", "Sun pitch", -90, 90)]
    public float sunPitch
    {
        get
        {
            float value = RenderSettings.sun.transform.rotation.eulerAngles.x;
            if (value >= 270)
                value -= 360;
            return value;
        }
        set
        {
            Vector3 eulerAngles = RenderSettings.sun.transform.rotation.eulerAngles;
            eulerAngles.x = value;
            RenderSettings.sun.transform.rotation = Quaternion.Euler(eulerAngles);

            UpdateEnvironment();
        }
    }
    [SliderProp("sya", "Sun yaw", 0, 360)]
    public float sunYaw
    {
        get => RenderSettings.sun.transform.rotation.eulerAngles.y;
        set
        {
            Vector3 eulerAngles = RenderSettings.sun.transform.rotation.eulerAngles;
            eulerAngles.y = value;
            RenderSettings.sun.transform.rotation = Quaternion.Euler(eulerAngles);

            UpdateSky();
        }
    }
    [SliderProp("sha", "Shadows", 0, 1)]
    public float shadowStrength
    {
        get => RenderSettings.sun.shadowStrength;
        set => RenderSettings.sun.shadowStrength = value;
    }
    [SliderProp("ref", "Reflections", 0, 1)]
    public float reflectionIntensity
    {
        get => GetReflectionProbe().intensity;
        set => GetReflectionProbe().intensity = value;
    }
    [SliderProp("fdn", "Fog density", 0, 1)]
    public float fogDensity
    {
        get => RenderSettings.fog ? Mathf.Sqrt(RenderSettings.fogDensity) : 0.0f;
        set
        {
            if (value == 0)
                RenderSettings.fog = false;
            else
            {
                RenderSettings.fog = true;
                RenderSettings.fogDensity = value * value;
            }
        }
    }
    [ColorProp("fco", "Fog color")]
    public Color fogColor
    {
        get => RenderSettings.fogColor;
        set => RenderSettings.fogColor = value;
    }

    public override PropertiesObjectType ObjectType()
    {
        return objectType;
    }

    public void SetSky(Material sky)
    {
        // instantiating material allows modifying the Rotation property without modifying asset
        var skyInstance = ResourcesDirectory.InstantiateMaterial(sky);
        skyInstance.name = sky.name; // sky will be saved correctly
        RenderSettings.skybox = skyInstance;
        UpdateSky();
    }

    private void UpdateSky()
    {
        var sky = RenderSettings.skybox;
        float baseRotation;
        if (sky != null && skyRotations.TryGetValue(sky.name, out baseRotation))
        {
            // rotate sky to match sun direction
            float yaw = RenderSettings.sun.transform.rotation.eulerAngles.y;
            sky.SetFloat(Shader.PropertyToID("_Rotation"), baseRotation - yaw + 180);
        }

        UpdateEnvironment();
    }

    private ReflectionProbe GetReflectionProbe()
    {
        // TODO!
        return GameObject.Find("ReflectionProbe")?.GetComponent<ReflectionProbe>();
    }

    private void UpdateEnvironment()
    {
        DynamicGI.UpdateEnvironment(); // update ambient lighting
        GetReflectionProbe().RenderProbe();
    }
}
