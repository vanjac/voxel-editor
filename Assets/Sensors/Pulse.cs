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
    private EntityReference resetInput = new EntityReference(null);

    public override PropertiesObjectType ObjectType()
    {
        return objectType;
    }

    public override ICollection<Property> Properties()
    {
        return Property.JoinProperties(new Property[]
        {
            new Property("sta", "Start on?",
                () => startOn,
                v => startOn = (bool)v,
                PropertyGUIs.Toggle),
            new Property("oft", "Off time",
                () => offTime,
                v => offTime = (float)v,
                PropertyGUIs.Time),
            new Property("ont", "On time",
                () => onTime,
                v => onTime = (float)v,
                PropertyGUIs.Time),
            new Property("rst", "Reset",
                () => resetInput,
                v => resetInput = (EntityReference)v,
                PropertyGUIs.EntityReferenceWithNull)
        }, base.Properties());
    }

    public override SensorComponent MakeComponent(GameObject gameObject)
    {
        PulseComponent pulse = gameObject.AddComponent<PulseComponent>();
        pulse.offTime = offTime;
        pulse.onTime = onTime;
        pulse.startOn = startOn;
        pulse.resetInput = resetInput;
        return pulse;
    }
}

public class PulseComponent : SensorComponent
{
    public bool startOn;
    public float offTime, onTime;
    public EntityReference resetInput;
    private float startTime;
    private bool resetWasOn = false;

    void Start()
    {
        startTime = Time.time;
    }

    public void Update()
    {
        bool resetIsOn = false;
        if (resetInput.component != null)
            resetIsOn = resetInput.component.IsOn();
        if (resetIsOn && !resetWasOn)
            startTime = Time.time;
        resetWasOn = resetIsOn;
        
        if (CheckState())
            AddActivator(null);
        else
            RemoveActivator(null);
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