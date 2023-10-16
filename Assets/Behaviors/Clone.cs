using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloneBehavior : TeleportBehavior
{
    public static new BehaviorType objectType = new BehaviorType(
        "Clone", "Create a copy of the object",
        "A new clone is created immediately when the behavior is activated. "
        + "The clone will start with the original health of the object. "
        + "Sensors which filter for a specific object will also activate for any of its clones.\n\n"
        // based on TeleportBehavior:
        + "•  <b>To:</b> Target location for clone\n"
        + "•  <b>Relative to:</b> Optional origin location. "
        + "If specified, the clone will be displaced from the original object by the difference between the target and origin.",
        "content-copy", typeof(CloneBehavior),
        BehaviorType.AndRule(
            BehaviorType.BaseTypeRule(typeof(DynamicEntity)),
            BehaviorType.NotBaseTypeRule(typeof(PlayerObject))));

    public override BehaviorType BehaviorObjectType()
    {
        return objectType;
    }

    public override Behaviour MakeComponent(GameObject gameObject)
    {
        var clone = gameObject.AddComponent<CloneComponent>();
        clone.Init(this);
        return clone;
    }
}

public class CloneComponent : BehaviorComponent<CloneBehavior>
{
    public override void BehaviorEnabled()
    {
        EntityComponent entityComponent = GetComponent<EntityComponent>();
        VoxelArray voxelArray = transform.parent.GetComponent<VoxelArray>();
        EntityComponent entityClone = entityComponent.entity.InitEntityGameObject(voxelArray, storeComponent: false);

        // based on TeleportComponent
        entityClone.transform.position = transform.position;
        if (behavior.target.component != null)
        {
            Vector3 originPos;
            if (behavior.origin.component != null)
                originPos = behavior.origin.component.transform.position;
            else
                originPos = transform.position;
            entityClone.transform.position += behavior.target.component.transform.position - originPos;
        }
    }
}