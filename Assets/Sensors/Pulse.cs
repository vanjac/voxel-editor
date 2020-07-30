using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PulseSensor : Sensor
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Pulse", "Turns on and off continuously",
        "Input is optional. When connected, it controls whether the pulse is active. "
        + "When the input turns off, the pulse completes a full cycle then stops.",
        "pulse", typeof(PulseSensor));

    private bool startOn = true;
    private float offTime = 1;
    private float onTime = 1;
    private EntityReference input = new EntityReference(null);

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

    public override SensorComponent MakeComponent(GameObject gameObject)
    {
        PulseComponent pulse = gameObject.AddComponent<PulseComponent>();
        pulse.offTime = offTime;
        pulse.onTime = onTime;
        pulse.startOn = startOn;
        pulse.input = input;
        return pulse;
    }
}

public class PulseComponent : SensorComponent
{
    public bool startOn;
    public float offTime, onTime;
    public EntityReference input;
    private float startTime;
    private bool useInput;
    private bool cyclePaused;

    void Start()
    {
        startTime = Time.time;
        useInput = input.component != null;
        cyclePaused = useInput;
    }

    public void Update()
    {
        bool inputIsOn = false;
        if (input.component != null)
            inputIsOn = input.component.IsOn();

        float timePassed = Time.time - startTime;
        if (cyclePaused && inputIsOn)
        {
            cyclePaused = false;
            startTime = Time.time;
            timePassed = 0;
        }
        else if (useInput && timePassed >= offTime + onTime)
        {
            if (inputIsOn)
            {
                while (timePassed >= offTime + onTime)
                {
                    startTime += offTime + onTime;
                    timePassed -= offTime + onTime;
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
            float cycleTime = timePassed % (offTime + onTime);
            if (startOn)
                state = cycleTime < onTime;
            else
                state = cycleTime >= offTime;
            if (state)
                AddActivator(null);
            else
                RemoveActivator(null);
        }
    }
}