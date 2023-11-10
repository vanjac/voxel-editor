using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[EditorPreviewBehavior]
public class ReflectorBehavior : GenericEntityBehavior<ReflectorBehavior, ReflectorComponent>
{
    public static new BehaviorType objectType = new BehaviorType(
        "Reflector", "Add more realistic reflections to area",
        "Reflector captures an image of the surrounding area and uses it to simulate reflections.\n"
        + "•  Surfaces within <b>Range</b> distance are affected\n"
        + "•  <b>Intensity</b> controls the brightness of the reflections\n"
        + "•  When <b>Real-time</b> is checked, reflections will update continuously (expensive!)",
        "mirror", typeof(ReflectorBehavior));

    public float size = 35, intensity = 1;
    public bool realtime;

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