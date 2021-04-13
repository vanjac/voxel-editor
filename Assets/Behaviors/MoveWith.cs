using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveWithBehavior : EntityBehavior
{
    public static new BehaviorType objectType = new BehaviorType(
        "Move With", "Follow the motion of another object",
        "BUG: This behavior will block Move behaviors from working.",
        "move-resize-variant", typeof(MoveWithBehavior),
        BehaviorType.BaseTypeRule(typeof(DynamicEntity)));

    private EntityReference parent = new EntityReference(null);
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
                () => parent,
                v => parent = (EntityReference)v,
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
        component.parent = parent;
        component.followRotation = followRotation;
        return component;
    }
}

public class MoveWithComponent : MotionComponent
{
    public EntityReference parent;
    public bool followRotation;

    private Vector3 positionOffset;
    private Quaternion rotationOffset;

    public override void BehaviorEnabled()
    {
        if (parent.component == null)
        {
            positionOffset = transform.position;
            rotationOffset = transform.rotation;
        }
        else
        {
            if (followRotation)
            {
                positionOffset = parent.component.transform.InverseTransformPoint(transform.position);
                rotationOffset = Quaternion.Inverse(parent.component.transform.rotation) * transform.rotation;
            }
            else
            {
                positionOffset = transform.position - parent.component.transform.position;
            }
        }
    }

    public override Vector3 GetTranslateFixed()
    {
        if (parent.component == null
                || parent.component.transform.position == DynamicEntityComponent.KILL_LOCATION)
            return Vector3.zero;
        if (followRotation)
            return parent.component.transform.TransformPoint(positionOffset) - transform.position;
        else
            return parent.component.transform.position + positionOffset - transform.position;
    }

    public override Quaternion GetRotateFixed()
    {
        if (!followRotation || parent.component == null)
            return Quaternion.identity;
        return Quaternion.Inverse(transform.rotation) * parent.component.transform.rotation * rotationOffset;
    }
}