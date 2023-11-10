using System.Collections.Generic;

public class TapSensor : GenericSensor<TapSensor, TapComponent>
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Tap", "Detect player tapping the object",
        "Object has to be Solid to detect a tap, but it doesn't have to be Visible.\n\n"
        + "Activator: the player",
        "gesture-tap", typeof(TapSensor));
    public override PropertiesObjectType ObjectType => objectType;

    public float maxDistance = 3;

    public override ICollection<Property> Properties() =>
        Property.JoinProperties(new Property[]
        {
            new Property("dis", "Max distance",
                () => maxDistance,
                v => maxDistance = (float)v,
                PropertyGUIs.Float)
        }, base.Properties());
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