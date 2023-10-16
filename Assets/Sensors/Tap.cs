using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TapSensor : Sensor
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Tap", "Detect player tapping the object",
        "Object has to be Solid to detect a tap, but it doesn't have to be Visible.\n\n"
        + "Activator: the player",
        "gesture-tap", typeof(TapSensor));

    public float maxDistance = 3;

    public override PropertiesObjectType ObjectType()
    {
        return objectType;
    }

    public override ICollection<Property> Properties()
    {
        return Property.JoinProperties(new Property[]
        {
            new Property("dis", "Max distance",
                () => maxDistance,
                v => maxDistance = (float)v,
                PropertyGUIs.Float)
        }, base.Properties());
    }

    public override ISensorComponent MakeComponent(GameObject gameObject)
    {
        var tap = gameObject.AddComponent<TapComponent>();
        tap.Init(this);
        return tap;
    }
}

public class TapComponent : SensorComponent<TapSensor>
{
    public float Distance
    {
        get => sensor.maxDistance;
    }

    private EntityComponent player;

    // called by GameTouchControl
    public void TapStart(EntityComponent player)
    {
        this.player = player;
        AddActivator(player);
    }

    // called by GameTouchControl
    public void TapEnd()
    {
        RemoveActivator(player);
    }
}