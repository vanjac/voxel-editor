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

    [FloatProp("dis", "Max distance")]
    public float maxDistance { get; set; } = 3;

    public override PropertiesObjectType ObjectType()
    {
        return objectType;
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