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
                Substance substance = new Substance(VoxelArrayEditor.instance);
                substance.behaviors.Add(new VisibleBehavior());
                substance.behaviors.Add(new SolidBehavior());
                return substance;
            }),
        new PropertiesObjectType("Water",
            "A block of water that you can swim in",
            "water",
            typeof(Substance),
            () => {
                Substance substance = new Substance(VoxelArrayEditor.instance);
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
                Substance substance = new Substance(VoxelArrayEditor.instance);
                substance.sensor = new TouchSensor();
                substance.xRay = true;
                substance.defaultPaint = new VoxelFace();
                substance.defaultPaint.overlay = ResourcesDirectory.MakeCustomMaterial(Shader.Find("Standard"), true);
                substance.defaultPaint.overlay.color = new Color(0, 0, 1, 0.25f);
                return substance;
            }),
        new PropertiesObjectType("Glass",
            "Solid block of glass",
            "cube",
            typeof(Substance),
            () => {
                Substance substance = new Substance(VoxelArrayEditor.instance);
                substance.behaviors.Add(new VisibleBehavior());
                substance.behaviors.Add(new SolidBehavior());
                substance.defaultPaint = new VoxelFace();
                substance.defaultPaint.overlay = ResourcesDirectory.GetMaterial("GameAssets/Overlays/glass/TranslucentGlassSaftey");
                return substance;
            }),
        new PropertiesObjectType("Neuron",
            "Logic component",
            "thought-bubble",
            typeof(Substance),
            () => {
                Substance substance = new Substance(VoxelArrayEditor.instance);
                substance.sensor = new InputThresholdSensor();
                EntityBehavior visible = new VisibleBehavior();
                visible.condition = EntityBehavior.Condition.ON;
                substance.behaviors.Add(visible);
                substance.behaviors.Add(new SolidBehavior());
                substance.defaultPaint = new VoxelFace();
                substance.defaultPaint.overlay = ResourcesDirectory.MakeCustomMaterial(Shader.Find("Standard"), true);
                substance.defaultPaint.overlay.color = new Color(.09f, .38f, .87f, .8f);
                return substance;
            })
    };

    public static PropertiesObjectType[] sensors = new PropertiesObjectType[]
    {
        PropertiesObjectType.NONE,
        TouchSensor.objectType,
        TapSensor.objectType,
        InputThresholdSensor.objectType,
        ToggleSensor.objectType,
        PulseSensor.objectType,
        DelaySensor.objectType
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
        WaterBehavior.objectType
    };

    public static PropertiesObjectType[] entityFilterTypes = new PropertiesObjectType[]
    {
        Entity.objectType,
        Substance.objectType
    };

}