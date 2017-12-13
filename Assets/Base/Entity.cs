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

public interface Entity : PropertiesObject
{
    byte GetTag();

    ICollection<EntityAction> Actions();

    List<EntityEvent> EventList(); // can be null if events not supported

    List<Entity> BehaviorList(); // can be null if behaviors not supported
}

public class EntityTag
{
    public const byte GREY = 0;
    public const byte RED = 1;
    public const byte ORANGE = 2;
    public const byte YELLOW = 3;
    public const byte GREEN = 4;
    public const byte CYAN = 5;
    public const byte BLUE = 6;
    public const byte PURPLE = 7;
}

public struct EntityAction
{
    public string name;
    public PropertyGUI argumentGUI;

    public EntityAction(string name)
    {
        this.name = name;
        argumentGUI = PropertyGUIs.Empty;
    }

    public EntityAction(string name, PropertyGUI gui)
    {
        this.name = name;
        argumentGUI = gui;
    }
}

public abstract class EntityEvent : PropertiesObject
{
    public List<EntityOutput> outputs;

    public virtual string TypeName()
    {
        return "Event";
    }

    public virtual ICollection<Property> Properties()
    {
        return new Property[] { };
    }
}

public struct EntityOutput
{
    public const byte START = 0;
    public const byte DELAY = 1;
    public const byte INTERVAL = 2;
    public const byte END = 3;

    public Entity[] targetEntities;
    public bool activatorIsTarget;
    public string targetAction;
    public object actionArgument;

    public byte outputType;
    public float time;
}


public abstract class SimpleEntity : Entity
{
    List<EntityEvent> eventList = new List<EntityEvent>();

    public virtual string TypeName()
    {
        return "Entity";
    }

    public virtual byte GetTag()
    {
        return EntityTag.GREY;
    }

    public virtual ICollection<Property> Properties()
    {
        return new Property[] { };
    }

    public virtual ICollection<EntityAction> Actions()
    {
        return new EntityAction[] { };
    }

    public List<EntityEvent> EventList()
    {
        return eventList;
    }

    public virtual List<Entity> BehaviorList()
    {
        return null;
    }
}


public abstract class DynamicEntity : SimpleEntity
{
    List<Entity> behaviorList = new List<Entity>();

    byte tag = EntityTag.GREY;
    // only for editor; makes object transparent allowing you to zoom/select through it
    public bool xRay = false;
    public bool visible = true;
    public bool solid = true;

    public override byte GetTag()
    {
        return tag;
    }

    public override ICollection<Property> Properties()
    {
        return new Property[]
        {
            new Property("Tag",
                () => tag,
                v => tag = (byte)v,
                PropertyGUIs.Tag),
            new Property("X-Ray?",
                () => xRay,
                v => {xRay = (bool)v; UpdateEntity();},
                PropertyGUIs.Toggle),
            new Property("Visible?",
                () => visible,
                v => visible = (bool)v,
                PropertyGUIs.Toggle),
            new Property("Solid?",
                () => solid,
                v => solid = (bool)v,
                PropertyGUIs.Toggle)
        };
    }

    public override ICollection<EntityAction> Actions()
    {
        return new EntityAction[]
        {
            new EntityAction("Show"),
            new EntityAction("Hide")
        };
    }

    public override List<Entity> BehaviorList()
    {
        return behaviorList;
    }

    public virtual void UpdateEntity() { }
}


public abstract class EntityBehavior : SimpleEntity
{
    // properties that can be changed through Actions
    public virtual ICollection<Property> DynamicProperties()
    {
        return new Property[] { };
    }

    public override ICollection<Property> Properties()
    {
        return DynamicProperties();
    }

    public override ICollection<EntityAction> Actions()
    {
        var actions = new List<EntityAction>();
        foreach (Property property in DynamicProperties())
            actions.Add(new EntityAction("Set " + property.name, property.gui));
        return actions;
    }

}