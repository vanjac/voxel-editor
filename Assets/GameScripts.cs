using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScripts
{
    public static PropertiesObjectType FindTypeWithName(PropertiesObjectType[] types, string name)
    {
        for (int i = 0; i < types.Length; i++)
            if (types[i].fullName == name)
                return types[i];
        return null;
    }


    public static PropertiesObjectType[] entityTemplates = new PropertiesObjectType[]
    {
        new PropertiesObjectType("Solid Substance",
            "A block that is solid and opaque by default",
            "cube",
            typeof(Substance),
            () => {
                Substance substance = new Substance();
                substance.behaviors.Add(new VisibleBehavior());
                substance.behaviors.Add(new SolidBehavior());
                return substance;
            }),
        new PropertiesObjectType("Water",
            "A block of water that you can swim in",
            "water",
            typeof(Substance),
            () => {
                Substance substance = new Substance();
                substance.behaviors.Add(new VisibleBehavior());
                substance.behaviors.Add(new WaterBehavior());
                substance.defaultPaint = new VoxelFace();
                substance.defaultPaint.overlay = ResourcesDirectory.GetMaterial("GameAssets/Overlays/water/WaterBasicDaytime");
                return substance;
            }),
        new PropertiesObjectType("Trigger",
            "Invisible, non-solid block with a touch sensor",
            "vector-combine",
            typeof(Substance),
            () => {
                Substance substance = new Substance();
                substance.sensor = new TouchSensor();
                substance.xRay = true;
                substance.defaultPaint = new VoxelFace();
                substance.defaultPaint.overlay = ResourcesDirectory.GetMaterial("GameAssets/Overlays/Invisible");
                return substance;
            }),
        new PropertiesObjectType("Glass",
            "Solid block of glass",
            "cube",
            typeof(Substance),
            () => {
                Substance substance = new Substance();
                substance.behaviors.Add(new VisibleBehavior());
                substance.behaviors.Add(new SolidBehavior());
                substance.defaultPaint = new VoxelFace();
                substance.defaultPaint.overlay = ResourcesDirectory.MakeCustomMaterial(ColorMode.GLASS, true);
                substance.defaultPaint.overlay.color = new Color(1, 1, 1, 0.25f);
                return substance;
            })
    };

    public static PropertiesObjectType[] sensors = new PropertiesObjectType[]
    {
        PropertiesObjectType.NONE,
        TouchSensor.objectType,
        InputThresholdSensor.objectType,
        ToggleSensor.objectType,
        PulseSensor.objectType,
        DelaySensor.objectType,
        MotionSensor.objectType,
        TapSensor.objectType,
        InRangeSensor.objectType,
        InCameraSensor.objectType
    };

    public static BehaviorType[] behaviors = new BehaviorType[]
    {
        VisibleBehavior.objectType,
        SolidBehavior.objectType,
        PhysicsBehavior.objectType,
        MoveBehavior.objectType,
        SpinBehavior.objectType,
        TeleportBehavior.objectType,
        HurtHealBehavior.objectType,
        WaterBehavior.objectType,
        ForceBehavior.objectType,
        LightBehavior.objectType
    };

    public static PropertiesObjectType[] objects = new PropertiesObjectType[]
    {
        PlayerObject.objectType,
        BallObject.objectType
    };

    public static PropertiesObjectType[] objectTemplates = new PropertiesObjectType[]
    {
        new PropertiesObjectType(BallObject.objectType, () =>
        {
            BallObject ball = new BallObject();
            ball.behaviors.Add(new VisibleBehavior());
            ball.behaviors.Add(new SolidBehavior());
            return ball;
        }),
        new PropertiesObjectType("Light",
            "",
            "lightbulb-on",
            typeof(BallObject),
            () => {
                var ball = new BallObject();
                foreach (Property prop in ball.Properties())
                {
                    if (prop.name == "Material")
                    {
                        Material lightMat = ResourcesDirectory.MakeCustomMaterial(ColorMode.GLASS, true);
                        lightMat.color = new Color(1, 1, 1, 0.25f);
                        prop.setter(lightMat);
                        break;
                    }
                }
                ball.xRay = true;
                ball.behaviors.Add(new LightBehavior());
                return ball;
            }),
        new PropertiesObjectType("Neuron",
            "Logic component, glows when on.",
            "thought-bubble",
            typeof(BallObject),
            () => {
                var ball = new BallObject();
                // TODO: there should be a better way to set properties
                foreach (Property prop in ball.Properties())
                {
                    if (prop.name == "Material")
                    {
                        Material neuronMat = ResourcesDirectory.MakeCustomMaterial(ColorMode.GLASS, true);
                        neuronMat.color = new Color(.09f, .38f, .87f, .25f);
                        prop.setter(neuronMat);
                        break;
                    }
                }
                ball.sensor = new InputThresholdSensor();
                ball.behaviors.Add(new VisibleBehavior());
                ball.behaviors.Add(new SolidBehavior());
                var light = new LightBehavior();
                light.condition = EntityBehavior.Condition.ON;
                foreach (Property prop in light.Properties())
                {
                    if (prop.name == "Color")
                        prop.setter(new Color(.09f, .38f, .87f));
                    else if (prop.name == "Size")
                        prop.setter(2.0f);
                    else if (prop.name == "Intensity")
                        prop.setter(3.0f);
                }
                ball.behaviors.Add(light);
                return ball;
            })
    };

    public static PropertiesObjectType[] entityFilterTypes = new PropertiesObjectType[]
    {
        Entity.objectType,
        Substance.objectType,
        BallObject.objectType
    };

}