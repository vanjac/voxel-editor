using System.Collections.Generic;
using UnityEngine;

public class WorldProperties : PropertiesObject {
    private static readonly Dictionary<string, float> skyRotations = new Dictionary<string, float> {
        {"sky5X1", 102},
        {"sky5X2", 143},
        {"sky5X3", 7},
        {"sky5X4", 177},
        {"sky5X5", 196}
    };

    public static PropertiesObjectType objectType = new PropertiesObjectType(
            "World", typeof(WorldProperties)) {
        displayName = s => s.WorldName,
        description = s => s.WorldDesc,
        iconName = "earth",
    };
    public PropertiesObjectType ObjectType => objectType;

    public void SetSky(Material sky) {
        // instantiating material allows modifying the Rotation property without modifying asset
        var skyInstance = ResourcesDirectory.InstantiateMaterial(sky);
        skyInstance.name = sky.name; // sky will be saved correctly
        RenderSettings.skybox = skyInstance;
        UpdateSky();
    }

    private void UpdateSky() {
        var sky = RenderSettings.skybox;
        if (sky != null && skyRotations.TryGetValue(sky.name, out float baseRotation)) {
            // rotate sky to match sun direction
            float yaw = RenderSettings.sun.transform.rotation.eulerAngles.y;
            sky.SetFloat(Shader.PropertyToID("_Rotation"), baseRotation - yaw + 180);
        }

        UpdateEnvironment();
    }

    private ReflectionProbe GetReflectionProbe() {
        // TODO!
        GameObject probeObj = GameObject.Find("ReflectionProbe");
        return (probeObj != null) ? probeObj.GetComponent<ReflectionProbe>() : null;
    }

    private void UpdateEnvironment() {
        DynamicGI.UpdateEnvironment(); // update ambient lighting
        GetReflectionProbe().RenderProbe();
    }

    public IEnumerable<Property> Properties() =>
        new Property[] {
            new Property("sky", s => s.PropSky,
                () => RenderSettings.skybox,
                v => {
                    SetSky((Material)v);
                },
                PropertyGUIs.Material(MaterialType.Sky, false)),
            new Property("amb", s => s.PropAmbientLightIntensity,
                () => RenderSettings.ambientIntensity,
                v => RenderSettings.ambientIntensity = (float)v,
                PropertyGUIs.Slider(0, 3)),
            new Property("sin", s => s.PropSunIntensity,
                () => RenderSettings.sun.intensity,
                v => RenderSettings.sun.intensity = (float)v,
                PropertyGUIs.Slider(0, 3)),
            new Property("sco", s => s.PropSunColor,
                () => RenderSettings.sun.color,
                v => RenderSettings.sun.color = (Color)v,
                PropertyGUIs.Color),
            new Property("spi", s => s.PropSunPitch,
                () => {
                    float value = RenderSettings.sun.transform.rotation.eulerAngles.x;
                    if (value >= 270) {
                        value -= 360;
                    }
                    return value;
                },
                v => {
                    Vector3 eulerAngles = RenderSettings.sun.transform.rotation.eulerAngles;
                    eulerAngles.x = (float)v;
                    RenderSettings.sun.transform.rotation = Quaternion.Euler(eulerAngles);

                    UpdateEnvironment();
                },
                PropertyGUIs.Slider(-90, 90)),
            new Property("sya", s => s.PropSunYaw,
                () => RenderSettings.sun.transform.rotation.eulerAngles.y,
                v => {
                    Vector3 eulerAngles = RenderSettings.sun.transform.rotation.eulerAngles;
                    eulerAngles.y = (float)v;
                    RenderSettings.sun.transform.rotation = Quaternion.Euler(eulerAngles);

                    UpdateSky();
                },
                PropertyGUIs.Slider(0, 360)),
            new Property("sha", s => s.PropShadows,
                () => RenderSettings.sun.shadowStrength,
                v => RenderSettings.sun.shadowStrength = (float)v,
                PropertyGUIs.Slider(0, 1)),
            new Property("ref", s => s.PropReflections,
                () => GetReflectionProbe().intensity,
                v => GetReflectionProbe().intensity = (float)v,
                PropertyGUIs.Slider(0, 1)),
            new Property("fog", s => s.PropFog,
                () => RenderSettings.fog,
                v => RenderSettings.fog = (bool)v,
                PropertyGUIs.Toggle),
            new Property("fd2", s => s.PropFogDensity,
                () => Mathf.Sqrt(RenderSettings.fogDensity),
                v => {
                    float value = (float)v;
                    RenderSettings.fogDensity = value * value;
                },
                PropertyGUIs.Slider(0, 1)),
            new Property("fco", s => s.PropFogColor,
                () => RenderSettings.fogColor,
                v => RenderSettings.fogColor = (Color) v,
                PropertyGUIs.Color)
        };

    public IEnumerable<Property> DeprecatedProperties() =>
        new Property[] {
            new Property("fdn", GUIStringSet.Empty,
                () => RenderSettings.fog ? Mathf.Sqrt(RenderSettings.fogDensity) : 0.0f,
                v => {
                    float value = (float)v;
                    if (value == 0) {
                        RenderSettings.fog = false;
                        RenderSettings.fogDensity = 0.04f;
                    } else {
                        RenderSettings.fog = true;
                        RenderSettings.fogDensity = value * value;
                    }
                },
                PropertyGUIs.Slider(0, 1)),
        };
}
