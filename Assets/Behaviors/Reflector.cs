using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[EditorPreviewBehavior]
public class ReflectorBehavior : EntityBehavior
{
    public static new BehaviorType objectType = new BehaviorType(
        "Reflector", "Put in the middle of a room for more realistic reflections",
        "mirror", typeof(ReflectorBehavior));
    
    private float size = 35, intensity = 1;
    private bool realtime;

    public override BehaviorType BehaviorObjectType()
    {
        return objectType;
    }

    public override ICollection<Property> Properties()
    {
        return Property.JoinProperties(base.Properties(), new Property[]
        {
            new Property("siz", "Range",
                () => size,
                v => size = (float)v,
                PropertyGUIs.Slider(1, 100)),
            new Property("int", "Intensity",
                () => intensity,
                v => intensity = (float)v,
                PropertyGUIs.Slider(0, 1.5f)),
            new Property("upd", "Real-time?",
                () => realtime,
                v => realtime = (bool)v,
                PropertyGUIs.Toggle)
        });
    }

    public override Behaviour MakeComponent(GameObject gameObject)
    {
        var component = gameObject.AddComponent<ReflectorComponent>();
        component.size = size;
        component.intensity = intensity;
        component.realtime = realtime;
        return component;
    }
}

public class ReflectorComponent : BehaviorComponent
{
    public float size, intensity;
    public bool realtime;

    private ReflectionProbe probe;
    private Vector3 prevPos;

    public override void Start()
    {
        probe = gameObject.AddComponent<ReflectionProbe>();
        probe.size = Vector3.one * size;
        probe.intensity = intensity;
        probe.mode = ReflectionProbeMode.Realtime;
        if (realtime && !CompareTag("EditorPreview"))
            probe.refreshMode = ReflectionProbeRefreshMode.EveryFrame;
        else
            probe.refreshMode = ReflectionProbeRefreshMode.ViaScripting;
        probe.importance = 2;
        probe.enabled = false;
        base.Start();
    }

    public override void BehaviorEnabled()
    {
        probe.enabled = true;
        if (probe.refreshMode == ReflectionProbeRefreshMode.ViaScripting)
            probe.RenderProbe();
        prevPos = transform.position;
    }

    public override void BehaviorDisabled()
    {
        // TODO if the probe is disabled while rendering, it will break
        probe.enabled = false;
    }

    void Update()
    {
        if (transform.position != prevPos)
        {
            prevPos = transform.position;
            if (CompareTag("EditorPreview"))
                probe.RenderProbe();
        }
    }
}