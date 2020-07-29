﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreBehavior : EntityBehavior
{
    public static new BehaviorType objectType = new BehaviorType(
        "Score", "Add or subtract from the player's score",
        "counter", typeof(ScoreBehavior));
    
    private int amount = 10;

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
        PlayerComponent.instance.score += amount;
    }
}