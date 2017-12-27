using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public delegate object GetProperty();
public delegate void SetProperty(object value);
public delegate void PropertyGUI(Property property);

public struct Property
{
    public string name;
    public GetProperty getter;
    public SetProperty setter;
    public PropertyGUI gui;
    public object value
    {
        get
        {
            return getter();
        }
        set
        {
            if (!getter().Equals(value))
                setter(value);
        }
    }

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
    public const byte NUM_TAGS = 16;

    public static string TagToString(byte tag)
    {
        // interesting unicode symbols start at U+25A0
        return "■□▲△●○★☆♥♡♦♢♠♤♣♧".Substring(tag, 1);
    }

    public override string ToString()
    {
        return TagToString(tag) + " " + TypeName();
    }

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
    public Entity entity;

    private List<Behaviour> offComponents = new List<Behaviour>();
    private List<Behaviour> onComponents = new List<Behaviour>();

    private SensorComponent sensorComponent;
    private bool sensorWasOn;

    public virtual void Start()
    {
        if (entity.sensor != null)
            sensorComponent = entity.sensor.MakeComponent(gameObject);
        sensorWasOn = false;
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
        bool sensorIsOn = IsOn();
        if (sensorIsOn && !sensorWasOn)
            SetBehaviors(true);
        else if (!sensorIsOn && sensorWasOn)
            SetBehaviors(false);
        sensorWasOn = sensorIsOn;
    }

    private void SetBehaviors(bool on)
    {
        foreach (Behaviour onComponent in onComponents)
            onComponent.enabled = on;
        foreach (Behaviour offComponent in offComponents)
            offComponent.enabled = !on;
    }

    public bool IsOn()
    {
        return sensorComponent.IsOn();
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
    public virtual string TypeName()
    {
        return "Sensor";
    }

    public virtual ICollection<Property> Properties()
    {
        return new Property[] { };
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
