using UnityEngine;

public static class GameScripts
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
        new PropertiesObjectType("Solid Substance", typeof(Substance))
        {
            displayName = s => s.SolidSubstanceName,
            description = s => s.SolidSubstanceDesc,
            iconName = SolidBehavior.objectType.iconName,
            constructor = () => {
                Substance substance = new Substance();
                substance.behaviors.Add(new VisibleBehavior());
                substance.behaviors.Add(new SolidBehavior());
                return substance;
            },
        },
        new PropertiesObjectType("Water", typeof(Substance))
        {
            displayName = WaterBehavior.objectType.displayName,
            description = s => s.WaterSubstanceDesc,
            iconName = WaterBehavior.objectType.iconName,
            constructor = () => {
                Substance substance = new Substance();
                substance.behaviors.Add(new VisibleBehavior());
                substance.behaviors.Add(new WaterBehavior());
                substance.defaultPaint = new VoxelFace();
                substance.defaultPaint.overlay = ResourcesDirectory.FindMaterial("WaterBasicDaytime", true);
                return substance;
            },
        },
        new PropertiesObjectType("Trigger", typeof(Substance))
        {
            displayName = s => s.TriggerName,
            description = s => s.TriggerDesc,
            iconName = TouchSensor.objectType.iconName,
            constructor = () => {
                Substance substance = new Substance();
                substance.sensor = new TouchSensor();
                substance.xRay = true;
                substance.defaultPaint = new VoxelFace();
                substance.defaultPaint.overlay = ResourcesDirectory.FindMaterial("Invisible", true);
                return substance;
            },
        },
        new PropertiesObjectType("Glass", typeof(Substance))
        {
            displayName = s => s.GlassName,
            description = s => s.GlassDesc,
            iconName = "window-closed-variant",
            constructor = () => {
                Substance substance = new Substance();
                substance.behaviors.Add(new VisibleBehavior());
                substance.behaviors.Add(new SolidBehavior());
                substance.defaultPaint = new VoxelFace();
                substance.defaultPaint.overlay = ResourcesDirectory.InstantiateMaterial(
                    ResourcesDirectory.FindMaterial("GLASS_overlay", true));
                substance.defaultPaint.overlay.color = new Color(1, 1, 1, 0.25f);
                return substance;
            },
        },
    };

    public static PropertiesObjectType[] sensors = new PropertiesObjectType[]
    {
        PropertiesObjectType.NONE,

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

    public static PropertiesObjectType[][] sensorTabs = new PropertiesObjectType[][]
    {
        new PropertiesObjectType[] // Detect
        {
            PropertiesObjectType.NONE,
            TouchSensor.objectType,
            TapSensor.objectType,
            MotionSensor.objectType,
            InRangeSensor.objectType,
            InCameraSensor.objectType,
            CheckScoreSensor.objectType,
        },
        new PropertiesObjectType[] // Logic
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
        ScaleBehavior.objectType,
        // JoystickBehavior.objectType,

        VisibleBehavior.objectType,
        LightBehavior.objectType,
        HaloBehavior.objectType,
        ReflectorBehavior.objectType,

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

    public static string[] BehaviorTabNames(GUIStringSet s) =>
        new string[]
        {
            s.BehaviorsMotion,
            s.BehaviorsGraphics,
            s.BehaviorsLife,
            s.BehaviorsPhysics,
            s.BehaviorsSound,
        };

    public static BehaviorType[][] behaviorTabs = new BehaviorType[][]
    {
        new BehaviorType[]
        {
            MoveBehavior.objectType,
            SpinBehavior.objectType,
            LookAtBehavior.objectType,
            TeleportBehavior.objectType,
            MoveWithBehavior.objectType,
            ScaleBehavior.objectType,
            // JoystickBehavior.objectType,
        },
        new BehaviorType[]
        {
            VisibleBehavior.objectType,
            LightBehavior.objectType,
            HaloBehavior.objectType,
            ReflectorBehavior.objectType,
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
        BallObject.objectType,
        PropObject.objectType,
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
        new PropertiesObjectType(PropObject.objectType, () =>
        {
            PropObject prop = new PropObject();
            prop.behaviors.Add(new ScaleBehavior());
            prop.behaviors.Add(new VisibleBehavior());
            prop.behaviors.Add(new SolidBehavior());
            return prop;
        }),
        new PropertiesObjectType("Empty", typeof(BallObject))
        {
            displayName = s => s.EmptyName,
            description = s => s.EmptyDesc,
            iconName = "scan-helper",
            constructor = () => {
                var ball = new BallObject();
                ball.paint.material = null;
                ball.paint.overlay = ResourcesDirectory.InstantiateMaterial(
                    ResourcesDirectory.FindMaterial("MATTE_overlay", true));
                ball.paint.overlay.color = new Color(1, 0, 0, 0.5f);
                return ball;
            },
        },
        new PropertiesObjectType("Light", typeof(BallObject))
        {
            displayName = LightBehavior.objectType.displayName,
            description = s => s.LightObjectDesc,
            iconName = LightBehavior.objectType.iconName,
            constructor = () => {
                var ball = new BallObject();
                ball.paint.material = null;
                ball.paint.overlay = ResourcesDirectory.InstantiateMaterial(
                    ResourcesDirectory.FindMaterial("GLASS_overlay", true));
                ball.paint.overlay.color = new Color(1, 1, 1, 0.25f);
                ball.xRay = true;
                ball.behaviors.Add(new LightBehavior());
                return ball;
            },
        },
        new PropertiesObjectType("Reflector", typeof(BallObject))
        {
            displayName = ReflectorBehavior.objectType.displayName,
            description = ReflectorBehavior.objectType.description,
            iconName = ReflectorBehavior.objectType.iconName,
            constructor = () => {
                var ball = new BallObject();
                ball.paint.material = ResourcesDirectory.InstantiateMaterial(
                    ResourcesDirectory.FindMaterial("METAL", true));
                ball.paint.material.color = Color.white;
                ball.behaviors.Add(new ReflectorBehavior());
                return ball;
            },
        },
    };

    public static PropertiesObjectType[] entityFilterTypes = new PropertiesObjectType[]
    {
        Entity.objectType,
        Substance.objectType,
        BallObject.objectType,
        PropObject.objectType,
    };

}