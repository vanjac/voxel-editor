﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CheckScoreSensor : Sensor
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Check Score", "Active when the score is at or above/below a threshold",
        "code-greater-than-or-equal", typeof(CheckScoreSensor));
    
    public enum AboveOrBelow
    {
        ABOVE, BELOW
    }

    [EnumProp("cmp", "Score is")]
    public AboveOrBelow compare { get; set; } = AboveOrBelow.ABOVE;
    [IntProp("thr", "Threshold")]
    public int threshold { get; set; } = 100;

    public override PropertiesObjectType ObjectType()
    {
        return objectType;
    }

    public override SensorComponent MakeComponent(GameObject gameObject)
    {
        var component = gameObject.AddComponent<CheckScoreComponent>();
        component.threshold = threshold;
        component.compare = compare;
        return component;
    }
}

public class CheckScoreComponent : SensorComponent
{
    public int threshold;
    public CheckScoreSensor.AboveOrBelow compare;

    void Update()
    {
        var player = PlayerComponent.instance;
        if (player == null)
        {
            RemoveActivator(null);
        }
        else
        {
            if (compare == CheckScoreSensor.AboveOrBelow.ABOVE)
            {
                if (player.score >= threshold)
                    AddActivator(null);
                else
                    RemoveActivator(null);
            }
            else
            {
                if (player.score <= threshold)
                    AddActivator(null);
                else
                    RemoveActivator(null);
            }
        }
    }
}
