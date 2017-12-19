using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PulseSensor : Sensor
{
    public float offTime = 1;
    public float onTime = 1;

    public override string TypeName()
    {
        return "Pulse";
    }

    public override ICollection<Property> Properties()
    {
        return Property.JoinProperties(new Property[]
        {
            new Property("Off time",
                () => offTime,
                v => offTime = (float)v,
                PropertyGUIs.Time),
            new Property("On time",
                () => onTime,
                v => onTime = (float)v,
                PropertyGUIs.Time)
        }, base.Properties());
    }

    public override SensorComponent MakeComponent(GameObject gameObject)
    {
        PulseComponent pulse = gameObject.AddComponent<PulseComponent>();
        pulse.offTime = offTime;
        pulse.onTime = onTime;
        return pulse;
    }
}

public class PulseComponent : SensorComponent
{
    public float offTime, onTime;
    private float startTime;

    void Start()
    {
        startTime = Time.time;
    }

    public override bool isOn()
    {
        return (Time.time - startTime) % (offTime + onTime) >= offTime;
    }
}