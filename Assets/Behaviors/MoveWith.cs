﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveWithBehavior : EntityBehavior
{
    public static new BehaviorType objectType = new BehaviorType(
        "Move With", "Follow the motion of another object.",
        "move-resize-variant", typeof(MoveWithBehavior), BehaviorType.BaseTypeRule(typeof(DynamicEntity)));

    private EntityReference target = new EntityReference(null);

    public override BehaviorType BehaviorObjectType()
    {
        return objectType;
    }

    public override ICollection<Property> Properties()
    {
        return Property.JoinProperties(base.Properties(), new Property[]
        {
            new Property("Parent",
                () => target,
                v => target = (EntityReference)v,
                PropertyGUIs.EntityReference)
        });
    }

    public override Behaviour MakeComponent(GameObject gameObject)
    {
        MoveWithComponent component = gameObject.AddComponent<MoveWithComponent>();
        component.target = target;
        return component;
    }
}

public class MoveWithComponent : BehaviorComponent
{
    public EntityReference target;

    private Vector3 positionOffset;
    private Quaternion rotationOffset;

    public override void BehaviorEnabled()
    {
        if (target.component == null)
        {
            positionOffset = transform.position;
            rotationOffset = transform.rotation;
        }
        else
        {
            positionOffset = target.component.transform.InverseTransformPoint(transform.position);
            rotationOffset = Quaternion.Inverse(target.component.transform.rotation) * transform.rotation;
        }
    }

    void LateUpdate()
    {
        if (target.component != null)
        {
            transform.position = target.component.transform.TransformPoint(positionOffset);
            transform.rotation = target.component.transform.rotation * rotationOffset;
        }
    }
}