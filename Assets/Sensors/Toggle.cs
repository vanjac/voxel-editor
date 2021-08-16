using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleSensor : Sensor
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Toggle", "One input switches it on, one input switches it off",
        "If both inputs turn on simultaneously, the sensor toggles between on/off.\n\n"
        + "Activators: the activators of the <b>On input</b>, frozen when it is first turned on",
        "toggle-switch", typeof(ToggleSensor));

    [ToggleProp("sta", "Start on?")]
    public bool startOn { get; set; } = false;
    [EntityReferenceProp("ofi", "Off input", allowNull: true)]
    public EntityReference offInput { get; set; } = new EntityReference(null);
    [EntityReferenceProp("oni", "On input", allowNull: true)]
    public EntityReference onInput { get; set; } = new EntityReference(null);

    public override PropertiesObjectType ObjectType()
    {
        return objectType;
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