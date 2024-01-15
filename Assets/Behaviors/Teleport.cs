using System.Collections.Generic;
using UnityEngine;

public class TeleportBehavior : GenericEntityBehavior<TeleportBehavior, TeleportComponent>
{
    public static new BehaviorType objectType = new BehaviorType(
        "Teleport", typeof(TeleportBehavior))
    {
        description = s => s.TeleportDesc,
        longDescription = s => s.TeleportLongDesc,
        iconName = "send",
        rule = BehaviorType.BaseTypeRule(typeof(DynamicEntity)),
    };
    public override BehaviorType BehaviorObjectType => objectType;

    public EntityReference target = new EntityReference(null);
    public EntityReference origin = new EntityReference(null);

    public override IEnumerable<Property> Properties() =>
        Property.JoinProperties(base.Properties(), new Property[]
        {
            new Property("loc", s => s.PropTo,
                () => target,
                v => target = (EntityReference)v,
                PropertyGUIs.EntityReference),
            new Property("rel", s => s.PropRelativeTo,
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
                    var none = targetEntityIsActivator ? GUIPanel.StringSet.EntityRefActivator
                            : GUIPanel.StringSet.EntityRefNone;
                    PropertyGUIs._EntityReferenceCustom(property, targetEntityIsActivator, none);
                })
        });
}

public class TeleportComponent : BehaviorComponent<TeleportBehavior>
{
    public override void BehaviorEnabled()
    {
        if (behavior.target.component == null)
            return;
        Vector3 originPos;
        if (behavior.origin.component != null)
            originPos = behavior.origin.component.transform.position;
        else
            originPos = transform.position;
        transform.position += behavior.target.component.transform.position - originPos;
    }
}