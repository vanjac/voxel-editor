using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DelaySensor : Sensor
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Delay", "Adds a delay to an input turning on or off",
        "If the <b>Input</b> has been on for longer than the <b>On time</b>, sensor will turn on. "
        + "If the Input has been off for longer than the <b>Off time</b>, sensor will turn off. "
        + "If the Input cycles on/off faster than the on/off time, nothing happens.\n\n"
        + "Activators: the activators of the Input, added and removed with a delay",
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
            new Property("inp", "Input",
                () => input,
                v => input = (EntityReference)v,
                PropertyGUIs.EntityReference),
            new Property("oft", "Off time",
                () => offTime,
                v => offTime = (float)v,
                PropertyGUIs.Time),
            new Property("ont", "On time",
                () => onTime,
                v => onTime = (float)v,
                PropertyGUIs.Time),
            new Property("sta", "Start on?",
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
        component.startOn = startOn;
        return component;
    }
}

public class DelayComponent : SensorComponent
{
    private enum ActivatorState
    {
        TURNING_ON, ON, TURNING_OFF
    }

    private class DelayedActivator
    {
        public EntityComponent activator;
        public ActivatorState state;
        public float changeTime;

        public DelayedActivator(EntityComponent activator)
        {
            this.activator = activator;
        }
    }

    private class NullKeyComponent : EntityComponent { } // it's not abstract
    private static EntityComponent nullKey;

    private Dictionary<EntityComponent, DelayedActivator> delayedActivators = new Dictionary<EntityComponent, DelayedActivator>();
    private List<EntityComponent> delayedActivatorsToRemove = new List<EntityComponent>();
    private bool inputWasOn = false;

    public EntityReference input;
    public float onTime, offTime;
    public bool startOn;

    void Start()
    {
        if (nullKey == null)
        {
            var go = new GameObject();
            go.name = "Null Key";
            nullKey = go.AddComponent<NullKeyComponent>();
            nullKey.entity = new BallObject(); // idk
        }
        if (startOn)
        {
            var nullDelayedA = new DelayedActivator(null);
            nullDelayedA.state = ActivatorState.TURNING_OFF;
            nullDelayedA.changeTime = Time.time;
            AddActivator(null);
            delayedActivators[nullKey] = nullDelayedA;
        }
    }

    void Update()
    {
        float time = Time.time;
        EntityComponent inputEntity = input.component;

        foreach (var keyValue in delayedActivators)
        {
            DelayedActivator delayedA = keyValue.Value;
            if (delayedA.state == ActivatorState.TURNING_ON)
            {
                if (time - delayedA.changeTime >= onTime)
                {
                    delayedA.state = ActivatorState.ON;
                    AddActivator(delayedA.activator);
                }
            }
            else if (delayedA.state == ActivatorState.TURNING_OFF)
            {
                if (time - delayedA.changeTime >= offTime)
                {
                    delayedActivatorsToRemove.Add(keyValue.Key);
                    RemoveActivator(delayedA.activator);
                }
            }
        }
        foreach (EntityComponent e in delayedActivatorsToRemove)
            delayedActivators.Remove(e);
        delayedActivatorsToRemove.Clear();

        if (inputEntity == null)
            return;

        foreach (EntityComponent newActivator in inputEntity.GetNewActivators())
        {
            if (newActivator == null) // null is reserved for the net cycle
                continue;
            AddActivatorDelay(newActivator, newActivator, time);
        }

        foreach (EntityComponent removedActivator in inputEntity.GetRemovedActivators())
        {
            if (removedActivator == null) // null is reserved for the net cycle
                continue;
            RemoveActivatorDelay(removedActivator, removedActivator, time);
        }

        // net cycle
        bool inputIsOn = inputEntity.IsOn();
        if (inputIsOn && !inputWasOn)
            AddActivatorDelay(nullKey, null, time);
        else if (inputWasOn && !inputIsOn)
            RemoveActivatorDelay(nullKey, null, time);
        inputWasOn = inputIsOn;
    }

    private void AddActivatorDelay(EntityComponent key, EntityComponent newActivator, float time)
    {
        if (delayedActivators.ContainsKey(key))
        {
            // must be in TURNING_OFF state
            delayedActivators[key].state = ActivatorState.ON;
        }
        else
        {
            var delayedA = new DelayedActivator(newActivator);
            if (onTime == 0)
            {
                delayedA.state = ActivatorState.ON;
                AddActivator(newActivator);
            }
            else
            {
                delayedA.state = ActivatorState.TURNING_ON;
                delayedA.changeTime = time;
            }
            delayedActivators[key] = delayedA;
        }
    }

    private void RemoveActivatorDelay(EntityComponent key, EntityComponent removedActivator, float time)
    {
        if (delayedActivators.ContainsKey(key))
        {
            var delayedA = delayedActivators[key];
            if (delayedA.state == ActivatorState.TURNING_ON || offTime == 0)
            {
                delayedActivators.Remove(key);
                RemoveActivator(removedActivator);
            }
            else
            {
                delayedA.state = ActivatorState.TURNING_OFF;
                delayedA.changeTime = time;
            }
        }
    }
}