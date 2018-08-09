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
    private class DelayedActivator
    {
        public EntityComponent activator;
        public bool turnedOn = false;
        public float changeTime;
        public int activatorCount = 1;

        public DelayedActivator(EntityComponent activator)
        {
            this.activator = activator;
        }
    }
    private class NullDummyComponent : EntityComponent { } // it's not abstract

    private static EntityComponent nullDummyComponent;

    private Dictionary<EntityComponent, DelayedActivator> delayedActivators = new Dictionary<EntityComponent, DelayedActivator>();
    private List<EntityComponent> delayedActivatorsToRemove = new List<EntityComponent>();

    public EntityReference input;
    public float onTime, offTime;
    public bool startOn;

    void Start()
    {
        if (nullDummyComponent == null)
        {
            var go = new GameObject();
            go.name = "Null Dummy Component";
            nullDummyComponent = go.AddComponent<NullDummyComponent>();
            nullDummyComponent.entity = new BallObject(); // idk
        }
        if (startOn)
        {
            var nullDelayedA = new DelayedActivator(nullDummyComponent);
            nullDelayedA.turnedOn = true;
            nullDelayedA.activatorCount = 0;
            nullDelayedA.changeTime = Time.time;
            AddActivator(nullDelayedA.activator);
            delayedActivators[nullDelayedA.activator] = nullDelayedA;
        }
    }

    void Update()
    {
        float time = Time.time;

        EntityComponent inputEntity = input.component;
        if (inputEntity == null)
        {
            foreach (DelayedActivator delayedA in delayedActivators.Values)
            {
                if (delayedA.activatorCount != 0)
                {
                    delayedA.activatorCount = 0;
                    if (!delayedA.turnedOn || offTime == 0)
                    {
                        delayedActivatorsToRemove.Add(delayedA.activator);
                        RemoveActivator(delayedA.activator);
                    }
                    else
                    {
                        delayedA.changeTime = time;
                    }
                }
            }
            foreach (EntityComponent e in delayedActivatorsToRemove)
                delayedActivators.Remove(e);
            delayedActivatorsToRemove.Clear();
        }

        foreach (DelayedActivator delayedA in delayedActivators.Values)
        {
            if (delayedA.activatorCount > 0 && !delayedA.turnedOn)
            {
                if (time - delayedA.changeTime >= onTime)
                {
                    delayedA.turnedOn = true;
                    AddActivator(delayedA.activator);
                }
            }
            else if (delayedA.activatorCount == 0)
            {
                if (time - delayedA.changeTime >= offTime)
                {
                    delayedActivatorsToRemove.Add(delayedA.activator);
                    RemoveActivator(delayedA.activator);
                }
            }
        }
        foreach (EntityComponent e in delayedActivatorsToRemove)
            delayedActivators.Remove(e);
        delayedActivatorsToRemove.Clear();

        if (inputEntity == null)
            return;

        foreach (EntityComponent _newActivator in inputEntity.GetNewActivators())
        {
            EntityComponent newActivator = _newActivator;
            if (newActivator == null)
                newActivator = nullDummyComponent;
            if (delayedActivators.ContainsKey(newActivator))
            {
                delayedActivators[newActivator].activatorCount++;
            }
            else
            {
                var delayedA = new DelayedActivator(newActivator);
                if (onTime == 0)
                {
                    delayedA.turnedOn = true;
                    AddActivator(newActivator);
                }
                else
                {
                    delayedA.changeTime = time;
                }
                delayedActivators[newActivator] = delayedA;
            }

        }

        foreach (EntityComponent _removedActivator in inputEntity.GetRemovedActivators())
        {
            EntityComponent removedActivator = _removedActivator;
            if (removedActivator == null)
                removedActivator = nullDummyComponent;
            if (delayedActivators.ContainsKey(removedActivator))
            {
                var delayedA = delayedActivators[removedActivator];
                if (delayedA.activatorCount == 1)
                {
                    if (!delayedA.turnedOn || offTime == 0)
                    {
                        delayedActivatorsToRemove.Add(removedActivator);
                        RemoveActivator(removedActivator);
                    }
                    else
                    {
                        delayedA.changeTime = time;
                    }
                }
                if (delayedA.activatorCount > 0)
                    delayedA.activatorCount--;
            }
        }
        foreach (EntityComponent e in delayedActivatorsToRemove)
            delayedActivators.Remove(e);
        delayedActivatorsToRemove.Clear();
    }
}