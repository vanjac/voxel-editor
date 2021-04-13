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

    public override IEnumerable<Property> Properties()
    {
        return new Property[]
        {
            new Property("sky", "Sky",
                () => RenderSettings.skybox,
                v => {
                    SetSky((Material)v);
                },
                PropertyGUIs.Material("Skies", false)),
            new Property("amb", "Ambient light intensity",
                () => RenderSettings.ambientIntensity,
                v => RenderSettings.ambientIntensity = (float)v,
                PropertyGUIs.Slider(0, 3)),
            new Property("sin", "Sun intensity",
                () => RenderSettings.sun.intensity,
                v => RenderSettings.sun.intensity = (float)v,
                PropertyGUIs.Slider(0, 3)),
            new Property("sco", "Sun color",
                () => RenderSettings.sun.color,
                v => RenderSettings.sun.color = (Color)v,
                PropertyGUIs.Color),
            new Property("spi", "Sun pitch",
                () => {
                    float value = RenderSettings.sun.transform.rotation.eulerAngles.x;
                    if (value >= 270)
                        value -= 360;
                    return value;
                },
                v => {
                    Vector3 eulerAngles = RenderSettings.sun.transform.rotation.eulerAngles;
                    eulerAngles.x = (float)v;
                    RenderSettings.sun.transform.rotation = Quaternion.Euler(eulerAngles);

                    UpdateEnvironment();
                },
                PropertyGUIs.Slider(-90, 90)),
            new Property("sya", "Sun yaw",
                () => RenderSettings.sun.transform.rotation.eulerAngles.y,
                v => {
                    Vector3 eulerAngles = RenderSettings.sun.transform.rotation.eulerAngles;
                    eulerAngles.y = (float)v;
                    RenderSettings.sun.transform.rotation = Quaternion.Euler(eulerAngles);

                    UpdateSky();
                },
                PropertyGUIs.Slider(0, 360)),
            new Property("sha", "Shadows",
                () => RenderSettings.sun.shadowStrength,
                v => RenderSettings.sun.shadowStrength = (float)v,
                PropertyGUIs.Slider(0, 1)),
            new Property("ref", "Reflections",
                () => GetReflectionProbe().intensity,
                v => GetReflectionProbe().intensity = (float)v,
                PropertyGUIs.Slider(0, 1)),
            new Property("fdn", "Fog density",
                () => RenderSettings.fog ? Mathf.Sqrt(RenderSettings.fogDensity) : 0.0f,
                v => {
                    float value = (float)v;
                    if (value == 0)
                        RenderSettings.fog = false;
                    else
                    {
                        RenderSettings.fog = true;
                        RenderSettings.fogDensity = value * value;
                    }
                },
                PropertyGUIs.Slider(0, 1)),
            new Property("fco", "Fog color",
                () => RenderSettings.fogColor,
                v => RenderSettings.fogColor = (Color) v,
                PropertyGUIs.Color)
        };
    }
}
