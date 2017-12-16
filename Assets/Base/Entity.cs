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
}

public interface PropertiesObject
{
    string TypeName();
    ICollection<Property> Properties();
}

public abstract class Entity : PropertiesObject
{
    public Sensor sensor;
    public SensorSettings sensorSettings;
    public List<EntityBehavior> behaviors;
    public byte tag;

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
            new Property("Sensor Settings",
                () => sensorSettings,
                v => sensorSettings = (SensorSettings)v,
                PropertyGUIs.Empty)
        };
    }
}

public abstract class EntityBehavior : PropertiesObject
{
    public enum BehaviorCondition : byte
    {
        ON, OFF, BOTH
    }

    BehaviorCondition condition;
    Entity targetEntity; // null for self
    bool targetEntityIsActivator;

    public virtual string TypeName()
    {
        return "Behavior";
    }

    public virtual ICollection<Property> Properties()
    {
        return new Property[] { };
    }
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
}

public struct SensorSettings
{
    bool invert;
    float turnOnTime;
    float turnOffTime;
    float minOnTime;
    float maxOnTime;
    float minOffTime;
}


public abstract class DynamicEntity : Entity
{
    // only for editor; makes object transparent allowing you to zoom/select through it
    public bool xRay = false;
    public float health = 100;

    public override ICollection<Property> Properties()
    {
        List<Property> props = new List<Property>(base.Properties());
        props.AddRange(new Property[]
        {
            new Property("X-Ray?",
                () => xRay,
                v => {xRay = (bool)v; UpdateEntity();},
                PropertyGUIs.Toggle),
            new Property("Health",
                () => health,
                v => health = (float)v,
                PropertyGUIs.Empty)
        });
        return props;
    }

    public virtual void UpdateEntity() { }
}
