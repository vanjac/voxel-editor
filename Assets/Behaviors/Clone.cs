using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloneBehavior : EntityBehavior
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
        return gameObject.AddComponent<CloneComponent>();
    }
}

public class CloneComponent : BehaviorComponent
{
    public override void BehaviorEnabled()
    {
        EntityComponent entityComponent = GetComponent<EntityComponent>();
        VoxelArray voxelArray = transform.parent.GetComponent<VoxelArray>();
        entityComponent.entity.InitEntityGameObject(voxelArray, storeComponent: false);
    }
}