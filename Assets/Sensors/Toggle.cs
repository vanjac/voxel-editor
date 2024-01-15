using System.Collections.Generic;

public class ToggleSensor : GenericSensor<ToggleSensor, ToggleComponent>
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Toggle", typeof(ToggleSensor))
    {
        description = s => s.ToggleDesc,
        longDescription = s => s.ToggleLongDesc,
        iconName = "toggle-switch",
    };
    public override PropertiesObjectType ObjectType => objectType;

    public EntityReference offInput = new EntityReference(null);
    public EntityReference onInput = new EntityReference(null);
    public bool startOn;

    public override IEnumerable<Property> Properties() =>
        Property.JoinProperties(new Property[]
        {
            new Property("sta", s => s.PropStartOn,
                () => startOn,
                v => startOn = (bool)v,
                PropertyGUIs.Toggle),
            new Property("ofi", s => s.PropOffInput,
                () => offInput,
                v => offInput = (EntityReference)v,
                PropertyGUIs.EntityReferenceWithNull),
            new Property("oni", s => s.PropOnInput,
                () => onInput,
                v => onInput = (EntityReference)v,
                PropertyGUIs.EntityReferenceWithNull)
        }, base.Properties());
}

public class ToggleComponent : SensorComponent<ToggleSensor>
{
    public bool value;
    private bool bothOn = false;

    public override void Init(ToggleSensor sensor)
    {
        base.Init(sensor);
        value = sensor.startOn;
    }

    void Start()
    {
        if (value)
            // start on
            AddActivator(null);
    }

    void Update()
    {
        bool offInputOn = false;
        EntityComponent offEntity = sensor.offInput.component;
        if (offEntity != null)
            offInputOn = offEntity.IsOn();

        bool onInputOn = false;
        EntityComponent onEntity = sensor.onInput.component;
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