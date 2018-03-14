using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelaySensor : Sensor
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Delay", "Adds a delay to an input turning on or off", "timer",
        typeof(DelaySensor));

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

    void Update()
    {
        bool inputOn = false;
        Entity inputEntity = input.entity;
        if (inputEntity != null)
            inputOn = inputEntity.component.IsOn();
        switch (state)
        {
            case DelayState.OFF:
                if (inputOn)
                    if (onTime == 0)
                        state = DelayState.ON;
                    else
                    {
                        state = DelayState.TURNING_ON;
                        changeTime = Time.time;
                    }
                break;
            case DelayState.ON:
                if (!inputOn)
                    if (offTime == 0)
                        state = DelayState.OFF;
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
                    state = DelayState.ON;
                break;
            case DelayState.TURNING_OFF:
                if (inputOn)
                    state = DelayState.ON;
                else if (Time.time - changeTime >= offTime)
                    state = DelayState.OFF;
                break;
        }
    }

    public override bool IsOn()
    {
        return state == DelayState.ON || state == DelayState.TURNING_OFF;
    }
}