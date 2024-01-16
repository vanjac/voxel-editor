using UnityEngine;

public class CloneBehavior : TeleportBehavior
{
    public static new BehaviorType objectType = new BehaviorType("Clone", typeof(CloneBehavior))
    {
        displayName = s => s.CloneName,
        description = s => s.CloneDesc,
        longDescription = s => s.CloneLongDesc,
        iconName = "content-copy",
        rule = BehaviorType.AndRule(
            BehaviorType.BaseTypeRule(typeof(DynamicEntity)),
            BehaviorType.NotBaseTypeRule(typeof(PlayerObject))),
    };
    public override BehaviorType BehaviorObjectType => objectType;

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