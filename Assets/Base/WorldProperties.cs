using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldProperties : PropertiesObject
{
    public static PropertiesObjectType objectType = new PropertiesObjectType(
        "World", "Properties that affect the entire world", "earth", typeof(WorldProperties));

    public PropertiesObjectType ObjectType()
    {
        return objectType;
    }

    public ICollection<Property> Properties()
    {
        return new Property[]
        {
            new Property("Sky",
                () => RenderSettings.skybox,
                v => RenderSettings.skybox = (Material)v,
                PropertyGUIs.Material("GameAssets/Skies", "Unlit/Color")),
            new Property("Ambient light intensity",
                () => RenderSettings.ambientIntensity,
                v => RenderSettings.ambientIntensity = (float)v,
                PropertyGUIs.Slider(0, 3)),
            new Property("Sun intensity",
                () => RenderSettings.sun.intensity,
                v => RenderSettings.sun.intensity = (float)v,
                PropertyGUIs.Slider(0, 3)),
            new Property("Sun color",
                () => RenderSettings.sun.color,
                v => RenderSettings.sun.color = (Color)v,
                PropertyGUIs.Color),
            new Property("Sun pitch",
                () => {
                    float value = RenderSettings.sun.transform.rotation.eulerAngles.x;
                    if (value > 270)
                        value -= 360;
                    return value;
                },
                v => {
                    Vector3 eulerAngles = RenderSettings.sun.transform.rotation.eulerAngles;
                    eulerAngles.x = (float)v;
                    RenderSettings.sun.transform.rotation = Quaternion.Euler(eulerAngles);
                },
                PropertyGUIs.Slider(-90, 90)),
            new Property("Sun yaw",
                () => RenderSettings.sun.transform.rotation.eulerAngles.y,
                v => {
                    Vector3 eulerAngles = RenderSettings.sun.transform.rotation.eulerAngles;
                    eulerAngles.y = (float)v;
                    RenderSettings.sun.transform.rotation = Quaternion.Euler(eulerAngles);
                },
                PropertyGUIs.Slider(0, 360))
        };
    }
}
