using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TapSensor : Sensor
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Tap", "Detect player tapping the object",
        "Object doesn't have to be visible or solid to detect a tap.\n\n"
        + "Activator: the player",
        "gesture-tap", typeof(TapSensor));

    private float maxDistance = 3;

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

    public override SensorComponent MakeComponent(GameObject gameObject)
    {
        var tap = gameObject.AddComponent<TapComponent>();
        tap.maxDistance = maxDistance;
        return tap;
    }
}

public class TapComponent : SensorComponent
{
    public float maxDistance;
    private EntityComponent player;

    // called by GameTouchControl
    public void TapStart(EntityComponent player, float distance)
    {
        if (distance <= maxDistance)
        {
            this.player = player;
            AddActivator(player);
        }
    }

    // called by GameTouchControl
    public void TapEnd()
    {
        RemoveActivator(player);
    }
}