using System.Collections.Generic;

public class CheckScoreSensor : GenericSensor<CheckScoreSensor, CheckScoreComponent>
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Check Score", "Active when score is at or above/below a threshold",
        "code-greater-than-or-equal", typeof(CheckScoreSensor));
    public override PropertiesObjectType ObjectType => objectType;

    public enum AboveOrBelow
    {
        ABOVE, BELOW
    }

    public int threshold = 100;
    public AboveOrBelow compare = AboveOrBelow.ABOVE;

    public override IEnumerable<Property> Properties() =>
        Property.JoinProperties(new Property[]
        {
            new Property("cmp", "Score is",
                () => compare,
                v => compare = (AboveOrBelow)v,
                PropertyGUIs.Enum),
            new Property("thr", "Threshold",
                () => threshold,
                v => threshold = (int)v,
                PropertyGUIs.Int)
        }, base.Properties());
}

public class CheckScoreComponent : SensorComponent<CheckScoreSensor>
{
    void Update()
    {
        var player = PlayerComponent.instance;
        if (player == null)
        {
            RemoveActivator(null);
        }
        else
        {
            if (sensor.compare == CheckScoreSensor.AboveOrBelow.ABOVE)
            {
                if (player.score >= sensor.threshold)
                    AddActivator(null);
                else
                    RemoveActivator(null);
            }
            else
            {
                if (player.score <= sensor.threshold)
                    AddActivator(null);
                else
                    RemoveActivator(null);
            }
        }
    }
}
