using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportBehavior : EntityBehavior
{
    public static new BehaviorType objectType = new BehaviorType(
        "Teleport", "Instantly teleport to another location",
        "Properties:\n•  \"To\": Target location to teleport\n"
        + "•  \"Relative to\": Optional origin location. If specified, instead of going directly to the \"To\" target,"
        + " object will move the distance between \"Relative to\" and \"To\" targets.",
        "send", typeof(TeleportBehavior), BehaviorType.BaseTypeRule(typeof(DynamicEntity)));

    private EntityReference target = new EntityReference(null);
    private EntityReference origin = new EntityReference(null);

    public override BehaviorType BehaviorObjectType()
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
                        if (targetEntity.entity != null)
                            property.value = targetEntity;
                        else if (!targetEntityIsActivator)
                            // TODO: this is not a good solution
                            property.value = new EntityReference(
                                EntityReferencePropertyManager.CurrentEntity());
                    }
                    PropertyGUIs._EntityReferenceCustom(property, targetEntityIsActivator,
                        targetEntityIsActivator ? "Activator" : "None");
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

public class TeleportComponent : BehaviorComponent
{
    public EntityReference target;
    public EntityReference origin;

    public override void BehaviorEnabled()
    {
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