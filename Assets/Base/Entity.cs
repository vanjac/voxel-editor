using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate object GetProperty();
public delegate void SetProperty(object value);
public delegate object PropertyGUI(object value);

public struct Property
{
    public string name;
    public GetProperty getter;
    public SetProperty setter;
    public PropertyGUI gui;

    public Property(string name, GetProperty getter, SetProperty setter, PropertyGUI gui)
    {
        this.name = name;
        this.getter = getter;
        this.setter = setter;
        this.gui = gui;
    }

    public static ICollection<Property> JoinProperties(
        ICollection<Property> props1, ICollection<Property> props2)
    {
        var props = new List<Property>(props1);
        props.AddRange(props2);
        return props;
    }
}

public interface PropertiesObject
{
    string TypeName();
    ICollection<Property> Properties();
}

public abstract class Entity : PropertiesObject
{
    public EntityComponent component;
    public Sensor sensor;
    public List<EntityBehavior> behaviors = new List<EntityBehavior>();
    public byte tag = 0;

    public virtual string TypeName()
    {
        return "Entity";
    }

    public virtual ICollection<Property> Properties()
    {
        return new Property[]
        {
            new Property("Tag",
                () => tag,
                v => tag = (byte)v,
                PropertyGUIs.Tag),
        };
    }
}

public abstract class EntityComponent : MonoBehaviour
{
    private enum SensorCycle
    {
        OFF, TURNING_ON, ON, TURNING_OFF, TIMED_OUT
    }

    private static bool SensorOn(SensorCycle cycle)
    {
        return cycle == SensorCycle.TURNING_ON
            || cycle == SensorCycle.ON
            || cycle == SensorCycle.TIMED_OUT;
    }

    private static bool BehaviorsOn(SensorCycle cycle)
    {
        return cycle == SensorCycle.ON || cycle == SensorCycle.TURNING_OFF;
    }

    public Entity entity;

    private List<Behaviour> offComponents = new List<Behaviour>();
    private List<Behaviour> onComponents = new List<Behaviour>();

    private SensorComponent sensorComponent;
    private SensorCycle _sensorCycle;
    private SensorCycle sensorCycle
    {
        get
        {
            return _sensorCycle;
        }
        set
        {
            if (SensorOn(value) && !SensorOn(_sensorCycle))
                t_sensorOn = Time.time;
            if (BehaviorsOn(value) && !BehaviorsOn(_sensorCycle))
            {
                SetBehaviors(true);
                t_behaviorsOn = Time.time;
            }
            else if (!BehaviorsOn(value) && BehaviorsOn(_sensorCycle))
                SetBehaviors(false);
            _sensorCycle = value;
        }
    }
    private float t_behaviorsOn;
    private float t_sensorOn;

    public virtual void Start()
    {
        if (entity.sensor != null)
            sensorComponent = entity.sensor.MakeComponent(gameObject);
        sensorCycle = SensorCycle.OFF;
        foreach (EntityBehavior behavior in entity.behaviors)
        {
            Behaviour c = behavior.MakeComponent(gameObject);
            if (behavior.condition == EntityBehavior.Condition.OFF)
                offComponents.Add(c);
            else if (behavior.condition == EntityBehavior.Condition.ON)
            {
                onComponents.Add(c);
                c.enabled = false;
            }
        }
    }

    void Update()
    {
        if (sensorComponent == null)
            return;
        bool sensorIsOn = sensorComponent.IsOn() ^ entity.sensor.invert;

        // change cycle state based on sensor
        switch (sensorCycle)
        {
            case SensorCycle.OFF:
                if (sensorIsOn)
                    sensorCycle = SensorCycle.TURNING_ON;
                break;
            case SensorCycle.TURNING_ON:
                if (!sensorIsOn)
                    sensorCycle = SensorCycle.OFF;
                break;
            case SensorCycle.ON:
                if (!sensorIsOn)
                    sensorCycle = SensorCycle.TURNING_OFF;
                break;
            case SensorCycle.TURNING_OFF:
                if (sensorIsOn)
                    sensorCycle = SensorCycle.ON;
                break;
            case SensorCycle.TIMED_OUT:
                if (!sensorIsOn)
                    sensorCycle = SensorCycle.OFF;
                break;
        }

        float time = Time.time;

        // change cycle state based on time
        switch (sensorCycle)
        {
            case SensorCycle.TURNING_ON:
                if (time - t_sensorOn > entity.sensor.turnOnTime)
                    sensorCycle = SensorCycle.ON;
                break;
            case SensorCycle.ON:
                if (time - t_behaviorsOn > entity.sensor.maxOnTime)
                    sensorCycle = SensorCycle.TIMED_OUT;
                break;
            case SensorCycle.TURNING_OFF:
                // timeSinceChange is time since ON, not time since TURNING_OFF
                if (time - t_behaviorsOn > entity.sensor.minOnTime)
                    sensorCycle = SensorCycle.OFF;
                break;
        }
    } // Update()

    private void SetBehaviors(bool on)
    {
        foreach (Behaviour onComponent in onComponents)
            onComponent.enabled = on;
        foreach (Behaviour offComponent in offComponents)
            offComponent.enabled = !on;
    }

    public bool IsOn()
    {
        return BehaviorsOn(sensorCycle);
    }
}

public abstract class EntityBehavior : PropertiesObject
{
    public enum Condition : byte
    {
        ON=0, OFF=1, BOTH=2
    }

    public Condition condition = Condition.BOTH;
    public Entity targetEntity = null; // null for self
    public bool targetEntityIsActivator = false;

    public virtual string TypeName()
    {
        return "Behavior";
    }

    public virtual ICollection<Property> Properties()
    {
        return new Property[]
        {
            new Property("Condition",
                () => condition,
                v => condition = (Condition)v,
                PropertyGUIs.BehaviorCondition)
        };
    }

    public abstract Behaviour MakeComponent(GameObject gameObject);
}

public abstract class Sensor : PropertiesObject
{
    public bool invert = false;
    public float turnOnTime = 0;
    public float minOnTime = 0;
    public float maxOnTime = 9999;

    public virtual string TypeName()
    {
        return "Sensor";
    }

    public virtual ICollection<Property> Properties()
    {
        return new Property[]
        {
            new Property("Invert?",
                () => invert,
                v => invert = (bool)v,
                PropertyGUIs.Toggle),
            new Property("Turn on delay",
                () => turnOnTime,
                v => turnOnTime = (float)v,
                PropertyGUIs.Time),
            new Property("Min on time",
                () => minOnTime,
                v => minOnTime = (float)v,
                PropertyGUIs.Time),
            new Property("Max on time",
                () => maxOnTime,
                v => maxOnTime = (float)v,
                PropertyGUIs.Time)
        };
    }

    public abstract SensorComponent MakeComponent(GameObject gameObject);
}

public abstract class SensorComponent : MonoBehaviour
{
    public abstract bool IsOn();
}


public abstract class DynamicEntity : Entity
{
    // only for editor; makes object transparent allowing you to zoom/select through it
    public bool xRay = false;
    public float health = 100;

    public override ICollection<Property> Properties()
    {
        return Property.JoinProperties(base.Properties(), new Property[]
        {
            new Property("X-Ray?",
                () => xRay,
                v => {xRay = (bool)v; UpdateEntity();},
                PropertyGUIs.Toggle),
            new Property("Health",
                () => health,
                v => health = (float)v,
                PropertyGUIs.Float)
        });
    }

    public virtual void UpdateEntity() { }
}
