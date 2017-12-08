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
    public bool dynamic; // can change in game

    public EntityProperty(string name, GetProperty getter, SetProperty setter, PropertyGUI gui)
    {
        this.name = name;
        this.getter = getter;
        this.setter = setter;
        this.gui = gui;
        dynamic = true;
    }

    public EntityProperty(string name, GetProperty getter, SetProperty setter, PropertyGUI gui, bool dynamic)
    {
        this.name = name;
        this.getter = getter;
        this.setter = setter;
        this.gui = gui;
        this.dynamic = dynamic;
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
    public Entity targetEntity; // null for Self or Activator
    public bool targetEntityIsActivator;
    public string targetAction;
    public object actionArgument;

    // activator rule...
    public bool[] activatorTagsAllowed;
    public Entity[] activatorEntityList;
    public bool activatorEntityBlacklist;
    public string[] activatorTypeList; // also applies to behaviors
    public bool activatorTypeBlacklist;
}


public class SimpleEntity : Entity
{
    List<EntityOutput> outputList = new List<EntityOutput>();
    List<Entity> behaviorList = new List<Entity>();


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
        var actions = new List<EntityAction>();
        foreach (EntityProperty property in Properties())
            if (property.dynamic)
                actions.Add(new EntityAction("Set " + property.name, property.gui));
        return actions;
    }

    public virtual ICollection<EntityEvent> Events()
    {
        return new EntityEvent[] { };
    }

    public List<EntityOutput> OutputList()
    {
        return outputList;
    }

    public List<Entity> BehaviorList()
    {
        return behaviorList;
    }
}


public class DynamicEntity : SimpleEntity
{
    bool enabled = true;
    byte tag = EntityTag.GREY;
    // only for editor; makes object transparent allowing you to zoom/select through it
    bool xRay = false;
    bool visible = true;
    bool solid = true;

    public override byte GetTag()
    {
        return tag;
    }

    public override ICollection<EntityProperty> Properties()
    {
        return new EntityProperty[]
        {
            new EntityProperty("Enabled?",
                () => enabled,
                v => enabled = (bool)v,
                PropertyGUIs.Toggle),
            new EntityProperty("Tag",
                () => tag,
                v => tag = (byte)v,
                PropertyGUIs.Tag, false),
            new EntityProperty("X-Ray?",
                () => xRay,
                v => xRay = (bool)v,
                PropertyGUIs.Toggle, false),
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

    public override ICollection<EntityAction> Actions()
    {
        var actions = new List<EntityAction>(base.Actions());
        actions.AddRange(new EntityAction[]
        {
            new EntityAction("Destroy"),
            new EntityAction("Clone"),
            new EntityAction("Teleport"),
            new EntityAction("Teleport Relative")
        });
        return actions;
    }

    public override ICollection<EntityEvent> Events()
    {
        return new EntityEvent[]
        {
            new EntityEvent("Destroyed", true),
            new EntityEvent("Cloned", false),
            new EntityEvent("Start Touch", true),
            new EntityEvent("End Touch", true),
            new EntityEvent("Player Look Towards", false),
            new EntityEvent("Player Look Away", false),
            new EntityEvent("Player Use", false)
        };
    }
}