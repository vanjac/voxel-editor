using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PulseSensor : Sensor
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Pulse", "Turns on and off continuously", "pulse", typeof(PulseSensor));

    private bool startOn = false;
    private float offTime = 1;
    private float onTime = 1;

    public override PropertiesObjectType ObjectType()
    {
        return objectType;
    }

    public override ICollection<Property> Properties()
    {
        return Property.JoinProperties(new Property[]
        {
            new Property("Start on?",
                () => startOn,
                v => startOn = (bool)v,
                PropertyGUIs.Toggle),
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
        pulse.startOn = startOn;
        return pulse;
    }
}

public class PulseComponent : SensorComponent
{
    public bool startOn;
    public float offTime, onTime;
    private float startTime;
    private EntityComponent selfComponent;

    void Start()
    {
        startTime = Time.time;
        selfComponent = GetComponent<EntityComponent>();
    }

    public void Update()
    {
        if (CheckState())
            AddActivator(selfComponent);
        else
            RemoveActivator(selfComponent);
    }

    private bool CheckState()
    {
        float cycleTime = (Time.time - startTime) % (offTime + onTime);
        if (startOn)
            return cycleTime < onTime;
        else
            return cycleTime >= offTime;
    }
}