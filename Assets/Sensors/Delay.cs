using System.Collections.Generic;
using UnityEngine;

public class DelaySensor : GenericSensor<DelaySensor, DelayComponent>
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Delay", s => s.DelayDesc, s => s.DelayLongDesc, "timer", typeof(DelaySensor));
    public override PropertiesObjectType ObjectType => objectType;

    public EntityReference input = new EntityReference(null);
    public float onTime, offTime;
    public bool startOn;

    public override IEnumerable<Property> Properties() =>
        Property.JoinProperties(new Property[]
        {
            new Property("inp", s => s.PropInput,
                () => input,
                v => input = (EntityReference)v,
                PropertyGUIs.EntityReference),
            new Property("oft", s => s.PropOffTime,
                () => offTime,
                v => offTime = (float)v,
                PropertyGUIs.Time),
            new Property("ont", s => s.PropOnTime,
                () => onTime,
                v => onTime = (float)v,
                PropertyGUIs.Time),
            new Property("sta", s => s.PropStartOn,
                () => startOn,
                v => startOn = (bool)v,
                PropertyGUIs.Toggle)
        }, base.Properties());
}

public class DelayComponent : SensorComponent<DelaySensor>
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

    void Start()
    {
        if (nullKey == null)
        {
            var go = new GameObject();
            go.name = "Null Key";
            nullKey = go.AddComponent<NullKeyComponent>();
            nullKey.entity = new BallObject(); // idk
        }
        if (sensor.startOn)
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
        EntityComponent inputEntity = sensor.input.component;

        foreach (var keyValue in delayedActivators)
        {
            DelayedActivator delayedA = keyValue.Value;
            if (delayedA.state == ActivatorState.TURNING_ON)
            {
                if (time - delayedA.changeTime >= sensor.onTime)
                {
                    delayedA.state = ActivatorState.ON;
                    AddActivator(delayedA.activator);
                }
            }
            else if (delayedA.state == ActivatorState.TURNING_OFF)
            {
                if (time - delayedA.changeTime >= sensor.offTime)
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
            if (sensor.onTime == 0)
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
            if (delayedA.state == ActivatorState.TURNING_ON || sensor.offTime == 0)
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