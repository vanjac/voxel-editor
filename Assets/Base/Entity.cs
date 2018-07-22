﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
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
    public bool explicitType; // store object type with property in file

    public Property(string name, GetProperty getter, SetProperty setter, PropertyGUI gui)
    {
        this.name = name;
        this.getter = getter;
        this.setter = setter;
        this.gui = gui;
        explicitType = false;
    }

    public Property(string name, GetProperty getter, SetProperty setter, PropertyGUI gui,
        bool explicitType)
    {
        this.name = name;
        this.getter = getter;
        this.setter = setter;
        this.gui = gui;
        this.explicitType = explicitType;
    }

    public static ICollection<Property> JoinProperties(
        ICollection<Property> props1, ICollection<Property> props2)
    {
        var props = new List<Property>(props1);
        props.AddRange(props2);
        return props;
    }
}


// needs to support serialization since it is a part of ActivatedSensor.Filter
// PropertiesObjectType is only serialized with its full name
// after it is deserialized, it is replaced with the correct instance of PropertiesObjectType
// with all the missing data (this is done by Filter)
public class PropertiesObjectType
{
    public static readonly PropertiesObjectType NONE = new PropertiesObjectType("None", null);

    public delegate PropertiesObject PropertiesObjectConstructor();

    public readonly string fullName;
    [XmlIgnore]
    public readonly string description;
    [XmlIgnore]
    public readonly string longDescription;
    [XmlIgnore]
    public readonly string iconName;
    [XmlIgnore]
    public readonly Type type;
    private readonly PropertiesObjectConstructor constructor;

    private Texture _icon;
    public Texture icon
    {
        get
        {
            if (_icon == null && iconName.Length > 0)
                _icon = Resources.Load<Texture>("Icons/" + iconName);
            return _icon;
        }
    }

    // empty constructor for deserialization
    public PropertiesObjectType() { }

    public PropertiesObjectType(string fullName, Type type) {
        this.fullName = fullName;
        description = "";
        longDescription = "";
        iconName = "";
        this.type = type;
        constructor = DefaultConstructor;
    }

    public PropertiesObjectType(string fullName, string description, string iconName, Type type)
    {
        this.fullName = fullName;
        this.description = description;
        longDescription = "";
        this.iconName = iconName;
        this.type = type;
        constructor = DefaultConstructor;
    }

    public PropertiesObjectType(string fullName, string description, string longDescription, string iconName, Type type)
    {
        this.fullName = fullName;
        this.description = description;
        this.longDescription = longDescription;
        this.iconName = iconName;
        this.type = type;
        constructor = DefaultConstructor;
    }

    public PropertiesObjectType(string fullName, string description, string iconName,
        Type type, PropertiesObjectConstructor constructor)
    {
        this.fullName = fullName;
        this.description = description;
        longDescription = "";
        this.iconName = iconName;
        this.type = type;
        this.constructor = constructor;
    }

    public PropertiesObjectType(string fullName, string description, string iconName, string longDescription,
        Type type, PropertiesObjectConstructor constructor)
    {
        this.fullName = fullName;
        this.description = description;
        this.longDescription = longDescription;
        this.iconName = iconName;
        this.type = type;
        this.constructor = constructor;
    }

    public PropertiesObjectType(PropertiesObjectType baseType, PropertiesObjectConstructor newConstructor)
    {
        this.fullName = baseType.fullName;
        this.description = baseType.description;
        this.longDescription = baseType.longDescription;
        this.iconName = baseType.iconName;
        this.type = baseType.type;
        constructor = newConstructor;
    }

    private PropertiesObject DefaultConstructor()
    {
        if (type == null)
            return null;
        return (PropertiesObject)System.Activator.CreateInstance(type);
    }

    public PropertiesObject Create()
    {
        return constructor();
    }

    // assumes both objects are the same type and have the same order of properties
    public static void CopyProperties(PropertiesObject source, PropertiesObject dest,
        Entity findEntity=null, Entity replaceEntity=null)
    {
        var sourceEnumerator = source.Properties().GetEnumerator();
        var destEnumerator = dest.Properties().GetEnumerator();
        while (sourceEnumerator.MoveNext())
        {
            destEnumerator.MoveNext();
            var value = sourceEnumerator.Current.value;

            if (findEntity != null)
            {
                // change "Self" references...
                if (value is EntityReference)
                {
                    if (((EntityReference)value).entity == findEntity)
                        value = new EntityReference(replaceEntity);
                }
                else if (value is Target)
                {
                    if (((Target)value).entityRef.entity == findEntity)
                        value = new Target(replaceEntity);
                }

            }

            destEnumerator.Current.setter(value);
        }
    }
}

public interface PropertiesObject
{
    PropertiesObjectType ObjectType();
    ICollection<Property> Properties();
}

public abstract class Entity : PropertiesObject
{
    public static PropertiesObjectType objectType = new PropertiesObjectType(
        "Anything", "", "circle-outline", typeof(Entity));

    public EntityComponent component;
    public Sensor sensor;
    public List<EntityBehavior> behaviors = new List<EntityBehavior>();
    public byte tag = 0;
    public const byte NUM_TAGS = 8;

    public Guid guid = Guid.Empty; // set by EntityReference

    public static string TagToString(byte tag)
    {
        // interesting unicode symbols start at U+25A0
        // U+2700 symbols don't seem to work
        return "■▲●★♥♦♠♣".Substring(tag, 1);
    }

    public override string ToString()
    {
        return TagToString(tag) + " " + ObjectType().fullName;
    }

    public virtual PropertiesObjectType ObjectType()
    {
        return objectType;
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

    // storeComponent: whether the "component" variable should be set
    // this should usually be true, but false for clones
    public abstract EntityComponent InitEntityGameObject(VoxelArray voxelArray, bool storeComponent = true);

    public abstract Vector3 PositionInEditor();
    public abstract bool AliveInEditor();

    public virtual Entity Clone()
    {
        var newEntity = (Entity)(ObjectType().Create());
        PropertiesObjectType.CopyProperties(this, newEntity, this, newEntity);
        if (sensor != null)
        {
            newEntity.sensor = (Sensor)(sensor.ObjectType().Create());
            PropertiesObjectType.CopyProperties(sensor, newEntity.sensor, this, newEntity);
        }
        else
            newEntity.sensor = null; // in case the Object Type had a default sensor
        newEntity.behaviors.Clear(); // in case the Object Type had default behaviors
        foreach (var behavior in behaviors)
        {
            var newBehavior = (EntityBehavior)(behavior.ObjectType().Create());
            PropertiesObjectType.CopyProperties(behavior, newBehavior, this, newEntity);
            newBehavior.targetEntity = behavior.targetEntity;
            newBehavior.targetEntityIsActivator = behavior.targetEntityIsActivator;
            newEntity.behaviors.Add(newBehavior);
        }
        return newEntity;
    }
}

public abstract class EntityComponent : MonoBehaviour
{
    public Entity entity;

    private List<Behaviour> offComponents = new List<Behaviour>();
    private List<Behaviour> onComponents = new List<Behaviour>();
    private List<Behaviour> targetedComponents = new List<Behaviour>();
    private List<EntityBehavior> activatorBehaviors = new List<EntityBehavior>();
    private List<Behaviour> activatorComponents = new List<Behaviour>();

    private SensorComponent sensorComponent;
    private bool sensorWasOn;

    public static EntityComponent FindEntityComponent(GameObject obj)
    {
        EntityComponent component = obj.GetComponent<EntityComponent>();
        if (component != null)
            return component;
        Transform parent = obj.transform.parent;
        if (parent != null)
            return parent.GetComponent<EntityComponent>();
        return null;
    }

    public static EntityComponent FindEntityComponent(Component c)
    {
        return FindEntityComponent(c.gameObject);
    }

    public virtual void Start()
    {
        if (entity.sensor != null)
            sensorComponent = entity.sensor.MakeComponent(gameObject);
        sensorWasOn = false;
        foreach (EntityBehavior behavior in entity.behaviors)
        {
            if (behavior.targetEntityIsActivator)
            {
                activatorBehaviors.Add(behavior);
                continue;
            }

            Behaviour c;
            if (behavior.targetEntity.entity != null)
            {
                c = behavior.MakeComponent(behavior.targetEntity.entity.component.gameObject);
                targetedComponents.Add(c);
            }
            else
            {
                c = behavior.MakeComponent(gameObject);
            }
            if (behavior.condition == EntityBehavior.Condition.OFF)
            {
                offComponents.Add(c);
                c.enabled = true;
            }
            else if (behavior.condition == EntityBehavior.Condition.ON)
            {
                onComponents.Add(c);
                c.enabled = false;
            }
            else
            {
                c.enabled = true;
            }
        }
    }

    void Update()
    {
        if (sensorComponent == null)
            return;
        bool sensorIsOn = IsOn();
        // order is important. behaviors should be disabled before other behaviors are enabled,
        // especially if identical behaviors are being disabled/enabled
        if (sensorIsOn && !sensorWasOn)
        {
            foreach (Behaviour offComponent in offComponents)
                if (offComponent != null)
                    offComponent.enabled = false;
            foreach (Behaviour onComponent in onComponents)
                if (onComponent != null)
                    onComponent.enabled = true;
            EntityComponent activator = sensorComponent.GetActivator();
            if (activator != null)
            {
                foreach (EntityBehavior behavior in activatorBehaviors)
                {
                    if (!behavior.BehaviorObjectType().rule(activator.entity))
                        continue;
                    Behaviour c = behavior.MakeComponent(activator.gameObject);
                    activatorComponents.Add(c);
                    c.enabled = true;
                }
            }
        }
        else if (!sensorIsOn && sensorWasOn)
        {
            foreach (Behaviour onComponent in onComponents)
                if (onComponent != null)
                    onComponent.enabled = false;
            foreach (Behaviour offComponent in offComponents)
                if (offComponent != null)
                    offComponent.enabled = true;
            foreach (Behaviour activatorComponent in activatorComponents)
                if (activatorComponent != null)
                    Destroy(activatorComponent);
            activatorComponents.Clear();
        }
        sensorWasOn = sensorIsOn;
    }

    void OnDestroy()
    {
        foreach (Behaviour c in targetedComponents)
            if (c != null)
                Destroy(c);
        foreach (Behaviour c in activatorComponents)
            if (c != null)
                Destroy(c);
    }

    public bool IsOn()
    {
        if (sensorComponent == null)
            return false;
        return sensorComponent.IsOn();
    }

    public EntityComponent GetActivator()
    {
        if (sensorComponent == null)
            return null;
        return sensorComponent.GetActivator();
    }
}

public abstract class EntityBehavior : PropertiesObject
{
    public static BehaviorType objectType = new BehaviorType(
        "Behavior", typeof(EntityBehavior));

    public enum Condition : byte
    {
        ON=0, OFF=1, BOTH=2
    }

    public Condition condition = Condition.BOTH;
    public EntityReference targetEntity = new EntityReference(null); // null for self
    public bool targetEntityIsActivator = false;

    public PropertiesObjectType ObjectType()
    {
        return BehaviorObjectType();
    }

    public virtual BehaviorType BehaviorObjectType()
    {
        return objectType;
    }

    public virtual ICollection<Property> Properties()
    {
        var gui = targetEntityIsActivator ?
            (PropertyGUI)PropertyGUIs.ActivatorBehaviorCondition
            : (PropertyGUI)PropertyGUIs.BehaviorCondition;

        return new Property[]
        {
            new Property("Condition",
                () => condition,
                v => condition = (Condition)v,
                gui)
        };
    }

    public abstract Behaviour MakeComponent(GameObject gameObject);
}


public abstract class BehaviorComponent : MonoBehaviour
{
    private bool started = false;

    // called after object is created and first enabled
    public virtual void Start()
    {
        started = true;
        if (enabled)
            BehaviorEnabled();
    }

    public virtual void OnEnable()
    {
        if (started)
            BehaviorEnabled();
    }

    public virtual void OnDisable()
    {
        if (started)
            BehaviorDisabled();
    }

    public virtual void BehaviorEnabled() { }
    public virtual void BehaviorDisabled() { }
}


public class BehaviorType : PropertiesObjectType
{
    public delegate bool BehaviorRule(Entity checkEntity);

    public readonly BehaviorRule rule;

    public BehaviorType(string fullName, Type type)
        : base(fullName, type)
    {
        this.rule = DefaultRule;
    }

    public BehaviorType(string fullName, string description, string iconName, Type type)
        : base(fullName, description, iconName, type)
    {
        this.rule = DefaultRule;
    }

    public BehaviorType(string fullName, string description, string longDescription, string iconName, Type type)
        : base(fullName, description, longDescription, iconName, type)
    {
        this.rule = DefaultRule;
    }

    public BehaviorType(string fullName, string description, string iconName, Type type,
        BehaviorRule rule)
        : base(fullName, description, iconName, type)
    {
        this.rule = rule;
    }

    public BehaviorType(string fullName, string description, string longDescription, string iconName, Type type,
        BehaviorRule rule)
        : base(fullName, description, longDescription, iconName, type)
    {
        this.rule = rule;
    }

    private static bool DefaultRule(Entity checkEntity)
    {
        return true;
    }

    public static BehaviorRule AndRule(BehaviorRule r1, BehaviorRule r2)
    {
        return (Entity checkEntity) =>
        {
            return r1(checkEntity) && r2(checkEntity);
        };
    }

    public static BehaviorRule BaseTypeRule(Type baseType)
    {
        return (Entity checkEntity) =>
        {
            return baseType.IsAssignableFrom(checkEntity.GetType());
        };
    }

    public static BehaviorRule NotBaseTypeRule(Type baseType)
    {
        return (Entity checkEntity) =>
        {
            return !baseType.IsAssignableFrom(checkEntity.GetType());
        };
    }
}


public abstract class Sensor : PropertiesObject
{
    public static PropertiesObjectType objectType = new PropertiesObjectType(
        "Sensor", typeof(Sensor));

    public virtual PropertiesObjectType ObjectType()
    {
        return objectType;
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

    // will only be called when sensor turns on
    public virtual EntityComponent GetActivator()
    {
        return null;
    }
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
                v => {xRay = (bool)v; UpdateEntityEditor();},
                PropertyGUIs.Toggle),
            new Property("Health",
                () => health,
                v => health = (float)v,
                PropertyGUIs.Float)
        });
    }

    // update the DynamicEntity's appearance in the Editor
    public virtual void UpdateEntityEditor() { }
}

public abstract class DynamicEntityComponent : EntityComponent
{
    public float health;

    public void Hurt(float amount)
    {
        health -= amount;
        if (health <= 0)
        {
            health = 0;
            Die();
        }
    }

    public void Heal(float amount)
    {
        health += amount;
    }

    public void Die()
    {
        // move entity out of any touch sensors so they will have a chance to turn off before it's destroyed
        transform.position = new Vector3(9999, 9999, 9999);
        StartCoroutine(DestroyCoroutine());
    }

    private IEnumerator DestroyCoroutine()
    {
        yield return null;
        Destroy(gameObject);
    }
}