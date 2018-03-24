using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportBehavior : EntityBehavior
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Teleport", "Instantly teleport to another location",
        "send", typeof(TeleportBehavior));

    private EntityReference target = new EntityReference(null);
    private EntityReference origin = new EntityReference(null);

    public override PropertiesObjectType ObjectType()
    {
        return objectType;
    }

    public override ICollection<Property> Properties()
    {
        return Property.JoinProperties(base.Properties(), new Property[]
        {
            new Property("To",
                () => target,
                v => target = (EntityReference)v,
                PropertyGUIs.EntityReference),
            new Property("Relative to",
                () => origin,
                v => origin = (EntityReference)v,
                (Property property) => {
                    var reference = (EntityReference)property.value;
                    if (reference.entity == null)
                    {
                        // TODO: this is not a good solution
                        property.value = new EntityReference(
                            EntityReferencePropertyManager.CurrentEntity());
                    }
                    PropertyGUIs.EntityReference(property);
                })
        });
    }

    public override Behaviour MakeComponent(GameObject gameObject)
    {
        TeleportComponent component = gameObject.AddComponent<TeleportComponent>();
        component.target = target;
        component.origin = origin;
        return component;
    }
}

public class TeleportComponent : MonoBehaviour
{
    public EntityReference target;
    public EntityReference origin;
    private bool started = false;

    void Start()
    {
        started = true;
        if (enabled)
            OnEnable();
    }

    void OnEnable()
    {
        if (!started)
            return;
        if (target.component == null)
            return;
        Vector3 originPos;
        if (origin.component != null)
            originPos = origin.component.transform.position;
        else
            originPos = transform.position;
        transform.position += target.component.transform.position - originPos;
    }
}