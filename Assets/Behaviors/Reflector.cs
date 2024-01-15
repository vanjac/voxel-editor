using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[EditorPreviewBehavior]
public class ReflectorBehavior : GenericEntityBehavior<ReflectorBehavior, ReflectorComponent>
{
    public static new BehaviorType objectType = new BehaviorType(
        "Reflector", s => s.ReflectorDesc, s => s.ReflectorLongDesc, "mirror",
        typeof(ReflectorBehavior));
    public override BehaviorType BehaviorObjectType => objectType;

    public float size = 35, intensity = 1;
    public bool realtime;

    public override IEnumerable<Property> Properties() =>
        Property.JoinProperties(base.Properties(), new Property[]
        {
            new Property("siz", s => s.PropRange,
                () => size,
                v => size = (float)v,
                PropertyGUIs.Slider(1, 100)),
            new Property("int", s => s.PropIntensity,
                () => intensity,
                v => intensity = (float)v,
                PropertyGUIs.Slider(0, 1.5f)),
            new Property("upd", s => s.PropRealTime,
                () => realtime,
                v => realtime = (bool)v,
                PropertyGUIs.Toggle)
        });
}

public class ReflectorComponent : BehaviorComponent<ReflectorBehavior>
{
    private ReflectionProbe probe;
    private Vector3 prevPos;

    public override void Start()
    {
        probe = gameObject.AddComponent<ReflectionProbe>();
        probe.size = Vector3.one * behavior.size;
        probe.intensity = behavior.intensity;
        probe.mode = ReflectionProbeMode.Realtime;
        if (behavior.realtime && !CompareTag("EditorPreview"))
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