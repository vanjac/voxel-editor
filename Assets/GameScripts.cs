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
            "wall",
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
                substance.defaultPaint.overlay = ResourcesDirectory.FindMaterial("WaterBasicDaytime", true);
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
                substance.defaultPaint.overlay = ResourcesDirectory.FindMaterial("Invisible", true);
                return substance;
            }),
        new PropertiesObjectType("Glass",
            "Solid block of glass",
            "window-closed-variant",
            typeof(Substance),
            () => {
                Substance substance = new Substance();
                substance.behaviors.Add(new VisibleBehavior());
                substance.behaviors.Add(new SolidBehavior());
                substance.defaultPaint = new VoxelFace();
                substance.defaultPaint.overlay = ResourcesDirectory.InstantiateMaterial(
                    ResourcesDirectory.FindMaterial("GLASS_overlay", true));
                substance.defaultPaint.overlay.color = new Color(1, 1, 1, 0.25f);
                return substance;
            })
    };

    public static PropertiesObjectType[] sensors = new PropertiesObjectType[]
    {
        PropertiesObjectType.NONE,
        //TestSensor.objectType,

        TouchSensor.objectType,
        TapSensor.objectType,
        MotionSensor.objectType,
        InRangeSensor.objectType,
        InCameraSensor.objectType,
        CheckScoreSensor.objectType,

        InputThresholdSensor.objectType,
        ToggleSensor.objectType,
        PulseSensor.objectType,
        RandomPulseSensor.objectType,
        DelaySensor.objectType
    };

    public static string[] sensorTabNames = new string[] { "Detect", "Logic" };

    public static PropertiesObjectType[][] sensorTabs = new PropertiesObjectType[][]
    {
        new PropertiesObjectType[]
        {
            PropertiesObjectType.NONE,
            TouchSensor.objectType,
            TapSensor.objectType,
            MotionSensor.objectType,
            InRangeSensor.objectType,
            InCameraSensor.objectType,
            CheckScoreSensor.objectType,
        },
        new PropertiesObjectType[]
        {
            InputThresholdSensor.objectType,
            ToggleSensor.objectType,
            PulseSensor.objectType,
            RandomPulseSensor.objectType,
            DelaySensor.objectType
        }
    };

    public static BehaviorType[] behaviors = new BehaviorType[]
    {
        MoveBehavior.objectType,
        SpinBehavior.objectType,
        LookAtBehavior.objectType,
        TeleportBehavior.objectType,
        MoveWithBehavior.objectType,

        VisibleBehavior.objectType,
        LightBehavior.objectType,

        HurtHealBehavior.objectType,
        CloneBehavior.objectType,
        ScoreBehavior.objectType,

        SolidBehavior.objectType,
        PhysicsBehavior.objectType,
        CharacterBehavior.objectType,
        CarryableBehavior.objectType,
        WaterBehavior.objectType,
        ForceBehavior.objectType,

        SoundBehavior.objectType,
        Sound3DBehavior.objectType
    };

    public static string[] behaviorTabNames = new string[] { "Motion", "Graphics", "Life", "Physics", "Sound" };

    public static BehaviorType[][] behaviorTabs = new BehaviorType[][]
    {
        new BehaviorType[]
        {
            MoveBehavior.objectType,
            SpinBehavior.objectType,
            LookAtBehavior.objectType,
            TeleportBehavior.objectType,
            MoveWithBehavior.objectType
        },
        new BehaviorType[]
        {
            VisibleBehavior.objectType,
            LightBehavior.objectType,
        },
        new BehaviorType[]
        {
            HurtHealBehavior.objectType,
            CloneBehavior.objectType,
            ScoreBehavior.objectType,
        },
        new BehaviorType[]
        {
            SolidBehavior.objectType,
            PhysicsBehavior.objectType,
            CharacterBehavior.objectType,
            CarryableBehavior.objectType,
            WaterBehavior.objectType,
            ForceBehavior.objectType
        },
        new BehaviorType[]
        {
            SoundBehavior.objectType,
            Sound3DBehavior.objectType
        }
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
                ball.paint.material = null;
                ball.paint.overlay = ResourcesDirectory.InstantiateMaterial(
                    ResourcesDirectory.FindMaterial("GLASS_overlay", true));
                ball.paint.overlay.color = new Color(1, 1, 1, 0.25f);
                ball.xRay = true;
                ball.behaviors.Add(new LightBehavior());
                return ball;
            }),
        new PropertiesObjectType("Neuron",
            "Logic component, glows when on",
            "thought-bubble",
            typeof(BallObject),
            () => {
                var ball = new BallObject();
                ball.paint.material = null;
                ball.paint.overlay = ResourcesDirectory.InstantiateMaterial(
                    ResourcesDirectory.FindMaterial("GLASS_overlay", true));
                ball.paint.overlay.color = new Color(.09f, .38f, .87f, .25f);

                ball.sensor = new InputThresholdSensor();
                ball.behaviors.Add(new VisibleBehavior());
                ball.behaviors.Add(new SolidBehavior());

                var light = new LightBehavior();
                light.condition = EntityBehavior.Condition.ON;
                PropertiesObjectType.SetProperty(light, "col", new Color(.09f, .38f, .87f));
                PropertiesObjectType.SetProperty(light, "siz", 2.0f);
                PropertiesObjectType.SetProperty(light, "int", 3.0f);
                PropertiesObjectType.SetProperty(light, "hal", true);
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