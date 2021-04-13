using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportBehavior : EntityBehavior
{
    public static new BehaviorType objectType = new BehaviorType(
        "Teleport", "Instantly teleport to another location",
        "•  <b>To:</b> Target location to teleport\n"
        + "•  <b>Relative to:</b> Optional origin location. If specified, instead of going directly to the target, "
        + "the object will be displaced by the difference between the target and origin.",
        "send", typeof(TeleportBehavior), BehaviorType.BaseTypeRule(typeof(DynamicEntity)));

    protected EntityReference dest = new EntityReference(null);
    protected EntityReference origin = new EntityReference(null);

    public override BehaviorType BehaviorObjectType()
    {
        return objectType;
    }

    public override IEnumerable<Property> Properties()
    {
        return Property.JoinProperties(base.Properties(), new Property[]
        {
            new Property("loc", "To",
                () => dest,
                v => dest = (EntityReference)v,
                PropertyGUIs.EntityReference),
            new Property("rel", "Relative to",
                () => origin,
                v => origin = (EntityReference)v,
                (Property property) => {
                    var reference = (EntityReference)property.value;
                    // TODO: this is not a good solution
                    Entity targetEntity = target.GetEntity(
                        EntityReferencePropertyManager.CurrentEntity());
                    if (reference.entity == null && targetEntity != null)
                        property.value = new EntityReference(targetEntity);
                    PropertyGUIs._EntityReferenceCustom(property, targetEntity == null,
                        targetEntity == null ? "Activator" : "None");
                })
        });
    }

    public override Behaviour MakeComponent(GameObject gameObject)
    {
        TeleportComponent component = gameObject.AddComponent<TeleportComponent>();
        component.dest = dest;
        component.origin = origin;
        return component;
    }
}

public class TeleportComponent : BehaviorComponent
{
    public EntityReference dest;
    public EntityReference origin;

    public override void BehaviorEnabled()
    {
        if (dest.component == null)
            return;
        Vector3 originPos;
        if (origin.component != null)
            originPos = origin.component.transform.position;
        else
            originPos = transform.position;
        transform.position += dest.component.transform.position - originPos;
    }
}