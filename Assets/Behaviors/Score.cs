using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreBehavior : EntityBehavior
{
    public static new BehaviorType objectType = new BehaviorType(
        "Score", "Add or subtract from the player's score",
        "counter", typeof(ScoreBehavior));
    
    [IntProp("num", "Amount")]
    public int amount { get; set; } = 10;

    public override BehaviorType BehaviorObjectType()
    {
        return objectType;
    }

    public override Behaviour MakeComponent(GameObject gameObject)
    {
        var component = gameObject.AddComponent<ScoreComponent>();
        component.amount = amount;
        return component;
    }
}

public class ScoreComponent : BehaviorComponent
{
    public int amount;

    public override void BehaviorEnabled()
    {
        if (PlayerComponent.instance != null)  // not dead
            PlayerComponent.instance.score += amount;
    }
}
