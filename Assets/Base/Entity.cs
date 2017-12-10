using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface Entity
{
    string EntityTypeName();

    byte GetTag();

    ICollection<EntityProperty> Properties();
    ICollection<EntityAction> Actions();
    ICollection<EntityEvent> Events();

    List<EntityOutput> OutputList(); // can be null if outputs not supported

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

public delegate object GetProperty();
public delegate void SetProperty(object value);
public delegate object PropertyGUI(object value);

public struct EntityProperty
{
    public string name;
    public GetProperty getter;
    public SetProperty setter;
    public PropertyGUI gui;

    public EntityProperty(string name, GetProperty getter, SetProperty setter, PropertyGUI gui)
    {
        this.name = name;
        this.getter = getter;
        this.setter = setter;
        this.gui = gui;
    }
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

public struct EntityEvent
{
    public string name;
    public bool hasActivator;

    public EntityEvent(string name, bool hasActivator)
    {
        this.name = name;
        this.hasActivator = hasActivator;
    }
}

public struct EntityOutput
{
    public Entity[] targetEntities;
    public bool selfIsTarget;
    public bool activatorIsTarget;
    public string targetAction;
    public object actionArgument;

    // activator rule...
    public bool[] filterActivatorTags;
    public Type filterActivatorType;
    public Entity[] filterActivatorEntity;
}


public abstract class SimpleEntity : Entity
{
    List<EntityOutput> outputList = new List<EntityOutput>();

    public virtual string EntityTypeName()
    {
        return "Entity";
    }

    public virtual byte GetTag()
    {
        return EntityTag.GREY;
    }

    public virtual ICollection<EntityProperty> Properties()
    {
        return new EntityProperty[] { };
    }

    public virtual ICollection<EntityAction> Actions()
    {
        return new EntityAction[] { };
    }

    public virtual ICollection<EntityEvent> Events()
    {
        return new EntityEvent[] { };
    }

    public List<EntityOutput> OutputList()
    {
        return outputList;
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

    public override ICollection<EntityProperty> Properties()
    {
        return new EntityProperty[]
        {
            new EntityProperty("Tag",
                () => tag,
                v => tag = (byte)v,
                PropertyGUIs.Tag),
            new EntityProperty("X-Ray?",
                () => xRay,
                v => {xRay = (bool)v; UpdateEntity();},
                PropertyGUIs.Toggle),
            new EntityProperty("Visible?",
                () => visible,
                v => visible = (bool)v,
                PropertyGUIs.Toggle),
            new EntityProperty("Solid?",
                () => solid,
                v => solid = (bool)v,
                PropertyGUIs.Toggle)
        };
    }

    public override List<Entity> BehaviorList()
    {
        return behaviorList;
    }

    public virtual void UpdateEntity() { }
}


public abstract class Behavior : SimpleEntity
{
    // properties that can be changed through Actions
    public virtual ICollection<EntityProperty> DynamicProperties()
    {
        return new EntityProperty[] { };
    }

    public override ICollection<EntityProperty> Properties()
    {
        return DynamicProperties();
    }

    public override ICollection<EntityAction> Actions()
    {
        var actions = new List<EntityAction>();
        foreach (EntityProperty property in DynamicProperties())
            actions.Add(new EntityAction("Set " + property.name, property.gui));
        return actions;
    }

}