using System.Collections.Generic;
using UnityEngine;

public class VisibleBehavior : GenericEntityBehavior<VisibleBehavior, VisibleComponent>
{
    public static new BehaviorType objectType = new BehaviorType("Visible", typeof(VisibleBehavior))
    {
        description = s => s.VisibleDesc,
        iconName = "eye",
        rule = BehaviorType.AndRule(
            BehaviorType.BaseTypeRule(typeof(DynamicEntity)),
            BehaviorType.NotBaseTypeRule(typeof(PlayerObject))),
    };
    public override BehaviorType BehaviorObjectType => objectType;
}

public class VisibleComponent : BehaviorComponent<VisibleBehavior>
{
    private IEnumerable<Renderer> IterateRenderers()
    {
        foreach (Renderer childRenderer in GetComponentsInChildren<Renderer>())
            yield return childRenderer;
    }

    public override void BehaviorEnabled()
    {
        foreach (Renderer r in IterateRenderers())
            r.enabled = true;
    }

    public override void LastBehaviorDisabled()
    {
        foreach (Renderer r in IterateRenderers())
            r.enabled = false;
    }
}