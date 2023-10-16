using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleBehavior : EntityBehavior
{
    public static new BehaviorType objectType = new BehaviorType(
        "Scale", "Change size along each axis",
        "Substances are scaled around their <b>Pivot</b> point."
        + " For physics objects, mass will <i>not</i> change.",
        "resize", typeof(ScaleBehavior),
        BehaviorType.AndRule(
            BehaviorType.BaseTypeRule(typeof(DynamicEntity)),
            BehaviorType.NotBaseTypeRule(typeof(PlayerObject))));

    public Vector3 scale = Vector3.one;

    public override BehaviorType BehaviorObjectType()
    {
        return objectType;
    }

    public override ICollection<Property> Properties()
    {
        return Property.JoinProperties(base.Properties(), new Property[]
        {
            new Property("sca", "Factor",
                () => scale,
                v => scale = (Vector3)v,
                PropertyGUIs.Vector3),
        });
    }

    public override Behaviour MakeComponent(GameObject gameObject)
    {
        var component = gameObject.AddComponent<ScaleComponent>();
        component.Init(this);
        return component;
    }
}

public class ScaleComponent : BehaviorComponent<ScaleBehavior>
{
    private Vector3 storedScale;

    public override void BehaviorEnabled()
    {
        storedScale = transform.localScale;
        transform.localScale = behavior.scale;
    }

    public override void BehaviorDisabled()
    {
        transform.localScale = storedScale;
    }
}