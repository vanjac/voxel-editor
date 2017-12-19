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
    public abstract bool isOn();
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
