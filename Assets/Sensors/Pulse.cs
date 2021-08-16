using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PulseSensor : Sensor
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Pulse", "Turns on and off continuously",
        "<b>Input</b> is optional. When connected, it controls whether the pulse is active. "
        + "When the Input turns off, the pulse completes a full cycle then stops.",
        "pulse", typeof(PulseSensor));

    [ToggleProp("sta", "Start on?")]
    public bool startOn { get; set; } = true;
    [TimeProp("oft", "Off time")]
    public float offTime { get; set; } = 1;
    [TimeProp("ont", "On time")]
    public float onTime { get; set; } = 1;
    [EntityReferenceProp("inp", "Input", allowNull: true)]
    public EntityReference input { get; set; } = new EntityReference(null);

    public override PropertiesObjectType ObjectType()
    {
        return objectType;
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