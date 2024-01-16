using UnityEngine;

public class SolidBehavior : GenericEntityBehavior<SolidBehavior, SolidComponent>
{
    public static new BehaviorType objectType = new BehaviorType("Solid", typeof(SolidBehavior))
    {
        displayName = s => s.SolidName,
        description = s => s.SolidDesc,
        iconName = "wall",
        rule = BehaviorType.AndRule(
            BehaviorType.BaseTypeRule(typeof(DynamicEntity)),
            BehaviorType.NotBaseTypeRule(typeof(PlayerObject))),
    };
    public override BehaviorType BehaviorObjectType => objectType;
}

public class SolidComponent : BehaviorComponent<SolidBehavior>
{
    public override void BehaviorEnabled()
    {
        foreach (Collider c in GetComponentsInChildren<Collider>())
            c.isTrigger = false;
    }

    public override void LastBehaviorDisabled()
    {
        foreach (Collider c in GetComponentsInChildren<Collider>())
            c.isTrigger = true;
    }
}