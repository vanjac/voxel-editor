using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SolidBehavior : EntityBehavior
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Solid", "Blocks and collides with other objects", "cube", typeof(SolidBehavior));

    public override PropertiesObjectType ObjectType()
    {
        return objectType;
    }

    public override Behaviour MakeComponent(GameObject gameObject)
    {
        return gameObject.AddComponent<SolidComponent>();
    }
}

public class SolidComponent : MonoBehaviour
{
    private System.Collections.Generic.IEnumerable<Collider> IterateColliders()
    {
        Collider c = GetComponent<Collider>();
        if (c != null)
            yield return c;
        foreach (BoxCollider childCollider in GetComponentsInChildren<BoxCollider>())
            yield return childCollider;
    }

    void Start()
    {
        if (enabled)
            OnEnable();
        else
            OnDisable();
    }

    void OnEnable()
    {
        foreach (Collider c in IterateColliders())
            c.isTrigger = false;
    }

    void OnDisable()
    {
        foreach (Collider c in IterateColliders())
            c.isTrigger = true;
    }
}