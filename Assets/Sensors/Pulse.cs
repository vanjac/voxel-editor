﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PulseSensor : Sensor
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Pulse", "Turn on and off continuously",
        "<b>Input</b> is optional. When connected, it controls whether the pulse is active. "
        + "When the Input turns off, the pulse completes a full cycle then stops.",
        "pulse", typeof(PulseSensor));

    public bool startOn = true;
    public float offTime = 1;
    public float onTime = 1;
    public EntityReference input = new EntityReference(null);

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
            new Property("inp", "Input",
                () => input,
                v => input = (EntityReference)v,
                PropertyGUIs.EntityReferenceWithNull)
        }, base.Properties());
    }

    public override ISensorComponent MakeComponent(GameObject gameObject)
    {
        PulseComponent pulse = gameObject.AddComponent<PulseComponent>();
        pulse.Init(this);
        return pulse;
    }
}

public class PulseComponent : SensorComponent<PulseSensor>
{
    private float startTime;
    private bool useInput;
    private bool cyclePaused;

    void Start()
    {
        startTime = Time.time;
        useInput = sensor.input.component != null;
        cyclePaused = useInput;
    }

    public void Update()
    {
        bool inputIsOn = false;
        if (sensor.input.component != null)
            inputIsOn = sensor.input.component.IsOn();

        float timePassed = Time.time - startTime;
        if (cyclePaused && inputIsOn)
        {
            cyclePaused = false;
            startTime = Time.time;
            timePassed = 0;
        }
        else if (useInput && timePassed >= sensor.offTime + sensor.onTime)
        {
            if (inputIsOn)
            {
                while (timePassed >= sensor.offTime + sensor.onTime)
                {
                    startTime += sensor.offTime + sensor.onTime;
                    timePassed -= sensor.offTime + sensor.onTime;
                }
            }
            else
            {
                cyclePaused = true;
            }
        }

        if (cyclePaused)
            RemoveActivator(null);
        else
        {
            bool state;
            float cycleTime = timePassed % (sensor.offTime + sensor.onTime);
            if (sensor.startOn)
                state = cycleTime < sensor.onTime;
            else
                state = cycleTime >= sensor.offTime;
            if (state)
                AddActivator(null);
            else
                RemoveActivator(null);
        }
    }
}