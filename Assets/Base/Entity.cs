using System;
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
        iconName = "";
        this.type = type;
        constructor = DefaultConstructor;
    }

    public PropertiesObjectType(string fullName, string description, Type type)
    {
        this.fullName = fullName;
        this.description = description;
        iconName = "";
        this.type = type;
        constructor = DefaultConstructor;
    }

    public PropertiesObjectType(string fullName, string description, string iconName, Type type)
    {
        this.fullName = fullName;
        this.description = description;
        this.iconName = iconName;
        this.type = type;
        constructor = DefaultConstructor;
    }

    public PropertiesObjectType(string fullName, string description, string iconName,
        Type type, PropertiesObjectConstructor constructor)
    {
        this.fullName = fullName;
        this.description = description;
        this.iconName = iconName;
        this.type = type;
        this.constructor = constructor;
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
        "Entity", "Any object in the game", "circle-outline", typeof(Entity));

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

    public abstract void InitEntityGameObject();

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
            GameObject targetGameObject = gameObject;
            if (behavior.targetEntity.entity != null)
                targetGameObject = behavior.targetEntity.entity.component.gameObject;
            Behaviour c = behavior.MakeComponent(targetGameObject);
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
        }
        else if (!sensorIsOn && sensorWasOn)
        {
            foreach (Behaviour onComponent in onComponents)
                if (onComponent != null)
                    onComponent.enabled = false;
            foreach (Behaviour offComponent in offComponents)
                if (offComponent != null)
                    offComponent.enabled = true;
        }
        sensorWasOn = sensorIsOn;
    }

    public bool IsOn()
    {
        if (sensorComponent == null)
            return false;
        return sensorComponent.IsOn();
    }
}

public abstract class EntityBehavior : PropertiesObject
{
    public static PropertiesObjectType objectType = new PropertiesObjectType(
        "Behavior", typeof(EntityBehavior));

    public enum Condition : byte
    {
        ON=0, OFF=1, BOTH=2
    }

    public Condition condition = Condition.BOTH;
    public EntityReference targetEntity = new EntityReference(null); // null for self
    public bool targetEntityIsActivator = false;

    public virtual PropertiesObjectType ObjectType()
    {
        return objectType;
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
        Destroy(gameObject);
    }
}