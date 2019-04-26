using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleSensor : Sensor
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Toggle", "One input switches it on, one input switches it off",
        "If both inputs turn on simultaneously, the sensor toggles between on/off.\n\n"
        + "Activators: the activators of the On input, frozen when it is first turned on",
        "toggle-switch", typeof(ToggleSensor));

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
            new Property("sta", "Start on?",
                () => startOn,
                v => startOn = (bool)v,
                PropertyGUIs.Toggle),
            new Property("ofi", "Off input",
                () => offInput,
                v => offInput = (EntityReference)v,
                PropertyGUIs.EntityReferenceWithNull),
            new Property("oni", "On input",
                () => onInput,
                v => onInput = (EntityReference)v,
                PropertyGUIs.EntityReferenceWithNull)
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
    private bool bothOn = false;

    void Start()
    {
        if (value)
            // start on
            AddActivator(null);
    }

    void Update()
    {
        bool offInputOn = false;
        EntityComponent offEntity = offInput.component;
        if (offEntity != null)
            offInputOn = offEntity.IsOn();

        bool onInputOn = false;
        EntityComponent onEntity = onInput.component;
        if (onEntity != null)
            onInputOn = onEntity.IsOn();

        if (offInputOn && onInputOn)
        {
            if (!bothOn)
            {
                bothOn = true;
                value = !value;
                if (value)
                    AddActivators(onEntity.GetActivators());
                else
                    ClearActivators();
            }
        }
        else
        {
            bothOn = false;
            if (offInputOn)
            {
                value = false;
                ClearActivators();
            }
            else if (onInputOn)
            {
                if (!value)
                    AddActivators(onEntity.GetActivators());
                value = true;
            }
        }
    }
}