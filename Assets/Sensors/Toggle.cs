using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleSensor : Sensor
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Toggle", "One input switches it on, one input switches it off", "toggle-switch",
        typeof(ToggleSensor));

    private EntityReference offInput = new EntityReference(null);
    private EntityReference onInput = new EntityReference(null);
    private bool startOn;

    public override PropertiesObjectType ObjectType()
    {
        return objectType;
    }

    public override ICollection<Property> Properties()
    {
        return Property.JoinProperties(new Property[]
        {
            new Property("Start on?",
                () => startOn,
                v => startOn = (bool)v,
                PropertyGUIs.Toggle),
            new Property("Off Input",
                () => offInput,
                v => offInput = (EntityReference)v,
                PropertyGUIs.EntityReference),
            new Property("On Input",
                () => onInput,
                v => onInput = (EntityReference)v,
                PropertyGUIs.EntityReference)
        }, base.Properties());
    }

    public override SensorComponent MakeComponent(GameObject gameObject)
    {
        ToggleComponent component = gameObject.AddComponent<ToggleComponent>();
        component.offInput = offInput;
        component.onInput = onInput;
        component.value = startOn;
        return component;
    }
}

public class ToggleComponent : SensorComponent
{
    public EntityReference offInput;
    public EntityReference onInput;
    public bool value;

    void Update()
    {
        Entity offEntity = offInput.entity;
        if (offEntity != null && offEntity.component.IsOn())
            value = false;
        Entity onEntity = onInput.entity;
        if (onEntity != null && onEntity.component.IsOn())
            value = true;
    }

    public override bool IsOn()
    {
        return value;
    }
}