using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pulse : Sensor
{
    public float rate = 1;

    public override string TypeName()
    {
        return "Pulse";
    }

    public override ICollection<Property> Properties()
    {
        var props = new List<Property>(base.Properties());
        props.AddRange(new Property[]
        {
            new Property("Rate",
                () => rate,
                v => rate = (float)v,
                PropertyGUIs.Float)
        });
        return props;
    }

    public override SensorComponent MakeComponent(GameObject gameObject)
    {
        PulseComponent pulse = gameObject.AddComponent<PulseComponent>();
        pulse.rate = rate;
        return pulse;
    }
}

public class PulseComponent : SensorComponent
{
    public float rate;
    private float startTime;

    void Start()
    {
        startTime = Time.time;
    }

    public override bool isOn()
    {
        return (Time.time - startTime) % (rate * 2) > rate;
    }
}