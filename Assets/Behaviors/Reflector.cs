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
    
    [SliderProp("siz", "Range", 1, 100)]
    public float size { get; set; } = 35;
    [SliderProp("int", "Intensity", 0, 1.5f)]
    public float intensity { get; set; } = 1;
    [ToggleProp("upd", "Real-time?")]
    public bool realtime { get; set; } = false;

    public override BehaviorType BehaviorObjectType()
    {
        return objectType;
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