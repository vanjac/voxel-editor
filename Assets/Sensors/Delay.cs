using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelaySensor : Sensor
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Delay", "Adds a delay to an input turning on or off",
        "If the input has been on for longer than the \"On time\", sensor will turn on. "
        + "If the input has been off for longer than the \"Off time\", sensor will turn off. "
        + "If the input cycles on/off faster than the on/off time, nothing happens.\n\n"
        + "Activators: the activators of the input, continuously updating",
        "timer", typeof(DelaySensor));

    private EntityReference input = new EntityReference(null);
    private float onTime, offTime;
    private bool startOn;

    public override PropertiesObjectType ObjectType()
    {
        return objectType;
    }

    public override ICollection<Property> Properties()
    {
        return Property.JoinProperties(new Property[]
        {
            new Property("Input",
                () => input,
                v => input = (EntityReference)v,
                PropertyGUIs.EntityReference),
            new Property("Off time",
                () => offTime,
                v => offTime = (float)v,
                PropertyGUIs.Time),
            new Property("On time",
                () => onTime,
                v => onTime = (float)v,
                PropertyGUIs.Time),
            new Property("Start on?",
                () => startOn,
                v => startOn = (bool)v,
                PropertyGUIs.Toggle)
        }, base.Properties());
    }

    public override SensorComponent MakeComponent(GameObject gameObject)
    {
        DelayComponent component = gameObject.AddComponent<DelayComponent>();
        component.input = input;
        component.onTime = onTime;
        component.offTime = offTime;
        component.state = startOn ? DelayComponent.DelayState.ON : DelayComponent.DelayState.OFF;
        return component;
    }
}

public class DelayComponent : SensorComponent
{
    public EntityReference input;
    public float onTime, offTime;
    public enum DelayState
    {
        OFF, TURNING_ON, ON, TURNING_OFF
    }
    public DelayState state;
    private float changeTime;
    private EntityComponent activator;

    void Start()
    {
        // start on
        if (state == DelayState.ON || state == DelayState.TURNING_OFF)
            AddActivator(null);
    }

    void Update()
    {
        bool inputOn = false;
        EntityComponent inputEntity = input.component;
        if (inputEntity != null)
        {
            inputOn = inputEntity.IsOn();
            if (state == DelayState.ON || state == DelayState.TURNING_OFF)
            {
                AddActivators(inputEntity.GetNewActivators());
                RemoveActivators(inputEntity.GetRemovedActivators());
            }
        }
        switch (state)
        {
            case DelayState.OFF:
                if (inputOn)
                {
                    if (onTime == 0)
                    {
                        state = DelayState.ON;
                        AddActivator(null);
                        AddActivators(inputEntity.GetActivators());
                    }
                    else
                    {
                        state = DelayState.TURNING_ON;
                        changeTime = Time.time;
                    }
                }
                break;
            case DelayState.ON:
                if (!inputOn)
                    if (offTime == 0)
                    {
                        state = DelayState.OFF;
                        RemoveActivator(null);
                        ClearActivators();
                    }
                    else
                    {
                        state = DelayState.TURNING_OFF;
                        changeTime = Time.time;
                    }
                break;
            case DelayState.TURNING_ON:
                if (!inputOn)
                    state = DelayState.OFF;
                else if (Time.time - changeTime >= onTime)
                {
                    state = DelayState.ON;
                    AddActivator(null);
                    AddActivators(inputEntity.GetActivators());
                }
                break;
            case DelayState.TURNING_OFF:
                if (inputOn)
                    state = DelayState.ON;
                else if (Time.time - changeTime >= offTime)
                {
                    state = DelayState.OFF;
                    RemoveActivator(null);
                    ClearActivators();
                }
                break;
        }
    }
}