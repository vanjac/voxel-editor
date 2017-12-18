using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Visible : EntityBehavior
{
    public override string TypeName()
    {
        return "Visible";
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

    void OnEnable()
    {
        foreach (Renderer r in IterateRenderers())
            r.enabled = true;
    }

    void Start()
    {
        if (enabled)
            OnEnable();
        else
            OnDisable();
    }

    void OnDisable()
    {
        foreach (Renderer r in IterateRenderers())
            r.enabled = false;
    }
}