using System.Collections.Generic;

public class TapSensor : GenericSensor<TapSensor, TapComponent>
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Tap", typeof(TapSensor))
    {
        description = s => s.TapDesc,
        longDescription = s => s.TapLongDesc,
        iconName = "gesture-tap",
    };
    public override PropertiesObjectType ObjectType => objectType;

    public float maxDistance = 3;

    public override IEnumerable<Property> Properties() =>
        Property.JoinProperties(new Property[]
        {
            new Property("dis", s => s.PropMaxDistance,
                () => maxDistance,
                v => maxDistance = (float)v,
                PropertyGUIs.Float)
        }, base.Properties());
}

public class TapComponent : SensorComponent<TapSensor>
{
    public float Distance => sensor.maxDistance;

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