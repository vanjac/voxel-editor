using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Visible : EntityBehavior
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Visible", "Object is visible in the game", typeof(Visible));

    public override PropertiesObjectType ObjectType()
    {
        return objectType;
    }

    public override Behaviour MakeComponent(GameObject gameObject)
    {
        return gameObject.AddComponent<VisibleComponent>();
    }
}

public class VisibleComponent : MonoBehaviour
{
    private System.Collections.Generic.IEnumerable<Renderer> IterateRenderers()
    {
        Renderer r = GetComponent<Renderer>();
        if (r != null)
            yield return r;
        foreach (Renderer childRenderer in GetComponentsInChildren<Renderer>())
            yield return childRenderer;
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
        foreach (Renderer r in IterateRenderers())
            r.enabled = true;
    }

    void OnDisable()
    {
        foreach (Renderer r in IterateRenderers())
            r.enabled = false;
    }
}