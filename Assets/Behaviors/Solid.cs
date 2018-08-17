using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SolidBehavior : EntityBehavior
{
    public static new BehaviorType objectType = new BehaviorType(
        "Solid", "Blocks and collides with other objects",
        "cube", typeof(SolidBehavior),
        BehaviorType.AndRule(
            BehaviorType.BaseTypeRule(typeof(DynamicEntity)),
            BehaviorType.NotBaseTypeRule(typeof(PlayerObject))));

    public override BehaviorType BehaviorObjectType()
    {
        return objectType;
    }

    public override Behaviour MakeComponent(GameObject gameObject)
    {
        return gameObject.AddComponent<SolidComponent>();
    }
}

public class SolidComponent : BehaviorComponent
{
    private System.Collections.Generic.IEnumerable<Collider> IterateColliders()
    {
        Collider c = GetComponent<Collider>();
        if (c != null)
            yield return c;
        foreach (BoxCollider childCollider in GetComponentsInChildren<BoxCollider>())
            yield return childCollider;
    }

    public override void BehaviorEnabled()
    {
        foreach (Collider c in IterateColliders())
            c.isTrigger = false;
    }

    public override void LastBehaviorDisabled()
    {
        foreach (Collider c in IterateColliders())
            c.isTrigger = true;
    }
}