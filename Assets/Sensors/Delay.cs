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
        + "Activators: the activators of the input, added and removed with a delay",
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
            EntityComponent key = newActivator;
            if (key == null)
                key = nullKey;
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

        foreach (EntityComponent removedActivator in inputEntity.GetRemovedActivators())
        {
            EntityComponent key = removedActivator;
            if (key == null)
                key = nullKey;
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
}