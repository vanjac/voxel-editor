using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreBehavior : GenericEntityBehavior<ScoreBehavior, ScoreComponent>
{
    public static new BehaviorType objectType = new BehaviorType(
        "Score", "Add or subtract from player's score",
        "counter", typeof(ScoreBehavior));

    public int amount = 10;

    public override BehaviorType BehaviorObjectType()
    {
        return objectType;
    }

    public override ICollection<Property> Properties()
    {
        return Property.JoinProperties(base.Properties(), new Property[]
        {
            new Property("num", "Amount",
                () => amount,
                v => amount = (int)v,
                PropertyGUIs.Int)
        });
    }
}

public class ScoreComponent : BehaviorComponent<ScoreBehavior>
{
    public override void BehaviorEnabled()
    {
        if (PlayerComponent.instance != null)  // not dead
            PlayerComponent.instance.score += behavior.amount;
    }
}
