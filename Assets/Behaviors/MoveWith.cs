using System.Collections.Generic;
using UnityEngine;

public class MoveWithBehavior : GenericEntityBehavior<MoveWithBehavior, MoveWithComponent>
{
    public static new BehaviorType objectType = new BehaviorType(
        "Move With", s => s.MoveWithDesc, s => s.MoveWithLongDesc, "move-resize-variant",
        typeof(MoveWithBehavior),
        BehaviorType.BaseTypeRule(typeof(DynamicEntity)));
    public override BehaviorType BehaviorObjectType => objectType;

    public EntityReference target = new EntityReference(null);
    public bool followRotation = true;

    public override IEnumerable<Property> Properties() =>
        Property.JoinProperties(base.Properties(), new Property[]
        {
            new Property("par", s => s.PropParent,
                () => target,
                v => target = (EntityReference)v,
                PropertyGUIs.EntityReference),
            new Property("fro", s => s.PropFollowRotation,
                () => followRotation,
                v => followRotation = (bool)v,
                PropertyGUIs.Toggle)
        });
}

public class MoveWithComponent : MotionComponent<MoveWithBehavior>
{
    private Vector3 positionOffset;
    private Quaternion rotationOffset;

    public override void BehaviorEnabled()
    {
        if (behavior.target.component == null)
        {
            positionOffset = transform.position;
            rotationOffset = transform.rotation;
        }
        else
        {
            var targetTransform = behavior.target.component.transform;
            if (behavior.followRotation)
            {
                positionOffset = targetTransform.InverseTransformPoint(transform.position);
                rotationOffset = Quaternion.Inverse(targetTransform.rotation) * transform.rotation;
            }
            else
            {
                positionOffset = transform.position - targetTransform.position;
            }
        }
    }

    public override Vector3 GetTranslateFixed()
    {
        var targetTransform = behavior.target.component.transform;
        if (behavior.target.component == null
                || targetTransform.position == DynamicEntityComponent.KILL_LOCATION)
            return Vector3.zero;
        if (behavior.followRotation)
            return targetTransform.TransformPoint(positionOffset) - transform.position;
        else
            return targetTransform.position + positionOffset - transform.position;
    }

    public override Quaternion GetRotateFixed()
    {
        if (!behavior.followRotation || behavior.target.component == null)
            return Quaternion.identity;
        return Quaternion.Inverse(transform.rotation) * behavior.target.component.transform.rotation
            * rotationOffset;
    }
}