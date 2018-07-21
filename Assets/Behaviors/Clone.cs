using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloneBehavior : TeleportBehavior
{
    public static new BehaviorType objectType = new BehaviorType(
        "Clone", "Create a copy of the object at its original position",
        "A new clone is created immediately when the behavior is activated. "
        + "The clone is created at the object's original position, with its original health.",
        "content-copy", typeof(CloneBehavior));

    public override BehaviorType BehaviorObjectType()
    {
        return objectType;
    }

    public override Behaviour MakeComponent(GameObject gameObject)
    {
        var clone = gameObject.AddComponent<CloneComponent>();
        clone.target = target;
        clone.origin = origin;
        return clone;
    }
}

public class CloneComponent : BehaviorComponent
{
    public EntityReference target;
    public EntityReference origin;

    public override void BehaviorEnabled()
    {
        EntityComponent entityComponent = GetComponent<EntityComponent>();
        VoxelArray voxelArray = transform.parent.GetComponent<VoxelArray>();
        EntityComponent entityClone = entityComponent.entity.InitEntityGameObject(voxelArray, storeComponent: false);

        // based on TeleportComponent
        entityClone.transform.position = transform.position;
        if (target.component != null)
        {
            Vector3 originPos;
            if (origin.component != null)
                originPos = origin.component.transform.position;
            else
                originPos = transform.position;
            entityClone.transform.position += target.component.transform.position - originPos;
        }
    }
}