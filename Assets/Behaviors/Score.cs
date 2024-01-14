using System.Collections.Generic;

public class ScoreBehavior : GenericEntityBehavior<ScoreBehavior, ScoreComponent>
{
    public static new BehaviorType objectType = new BehaviorType(
        "Score", s => s.ScoreDesc, "counter", typeof(ScoreBehavior));
    public override BehaviorType BehaviorObjectType => objectType;

    public int amount = 10;

    public override IEnumerable<Property> Properties() =>
        Property.JoinProperties(base.Properties(), new Property[]
        {
            new Property("num", "Amount",
                () => amount,
                v => amount = (int)v,
                PropertyGUIs.Int)
        });
}

public class ScoreComponent : BehaviorComponent<ScoreBehavior>
{
    void Awake()
    {
        if (PlayerComponent.instance != null)
            PlayerComponent.instance.hasScore = true;
    }

    public override void BehaviorEnabled()
    {
        if (PlayerComponent.instance != null)  // not dead
        {
            PlayerComponent.instance.score += behavior.amount;
            PlayerComponent.instance.hasScore = true;
        }
    }
}
