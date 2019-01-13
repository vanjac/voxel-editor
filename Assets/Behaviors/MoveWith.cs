using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveWithBehavior : EntityBehavior
{
    public static new BehaviorType objectType = new BehaviorType(
        "Move With", "Follow the motion of another object.",
        "move-resize-variant", typeof(MoveWithBehavior),
        BehaviorType.AndRule(
            BehaviorType.BaseTypeRule(typeof(DynamicEntity)),
            BehaviorType.NotBaseTypeRule(typeof(PlayerObject))));

    private EntityReference target = new EntityReference(null);
    private bool followRotation = true;

    public override BehaviorType BehaviorObjectType()
    {
        return objectType;
    }

    public override ICollection<Property> Properties()
    {
        return Property.JoinProperties(base.Properties(), new Property[]
        {
            new Property("par", "Parent",
                () => target,
                v => target = (EntityReference)v,
                PropertyGUIs.EntityReference),
            new Property("fro", "Follow rotation?",
                () => followRotation,
                v => followRotation = (bool)v,
                PropertyGUIs.Toggle)
        });
    }

    public override Behaviour MakeComponent(GameObject gameObject)
    {
        MoveWithComponent component = gameObject.AddComponent<MoveWithComponent>();
        component.target = target;
        component.followRotation = followRotation;
        return component;
    }
}

public class MoveWithComponent : MotionComponent
{
    public EntityReference target;
    public bool followRotation;

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
            if (followRotation)
            {
                positionOffset = target.component.transform.InverseTransformPoint(transform.position);
                rotationOffset = Quaternion.Inverse(target.component.transform.rotation) * transform.rotation;
            }
            else
            {
                positionOffset = transform.position - target.component.transform.position;
            }
        }
    }

    public override Vector3 GetTranslateFixed()
    {
        if (target.component == null
                || target.component.transform.position == DynamicEntityComponent.KILL_LOCATION)
            return Vector3.zero;
        if (followRotation)
            return target.component.transform.TransformPoint(positionOffset) - transform.position;
        else
            return target.component.transform.position + positionOffset - transform.position;
    }

    public override Quaternion GetRotateFixed()
    {
        if (!followRotation || target.component == null)
            return Quaternion.identity;
        return Quaternion.Inverse(transform.rotation) * target.component.transform.rotation * rotationOffset;
    }
}