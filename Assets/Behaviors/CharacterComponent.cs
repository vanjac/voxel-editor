using System.Collections.Generic;
using UnityEngine;

public class CharacterBehavior : BasePhysicsBehavior
{
    public static new BehaviorType objectType = new BehaviorType(
        "Character", typeof(CharacterBehavior))
    {
        description = s => s.CharacterDesc,
        longDescription = s => s.CharacterLongDesc,
        iconName = "human",
        rule = BehaviorType.AndRule(
            BehaviorType.BaseTypeRule(typeof(DynamicEntity)),
            BehaviorType.NotBaseTypeRule(typeof(PlayerObject))),
    };
    public override BehaviorType BehaviorObjectType => objectType;

    public override IEnumerable<Property> Properties() =>
        Property.JoinProperties(base.Properties(), new Property[]
        {
            new Property("den", s => s.PropDensity,
                () => density,
                v => density = (float)v,
                PropertyGUIs.Float)
        });

    public override Behaviour MakeComponent(GameObject gameObject)
    {
        var component = gameObject.AddComponent<CharacterComponent>();
        component.Init(this);
        return component;
    }
}


public class CharacterComponent : PhysicsComponent
{
    public override void BehaviorEnabled()
    {
        base.BehaviorEnabled();
        GetComponent<DynamicEntityComponent>().isCharacter = true;
        var rigidBody = gameObject.GetComponent<Rigidbody>();
        if (rigidBody != null)
            rigidBody.constraints = RigidbodyConstraints.FreezeRotation;
    }
    public override void LastBehaviorDisabled()
    {
        base.LastBehaviorDisabled();
        GetComponent<DynamicEntityComponent>().isCharacter = false;
        var rigidBody = gameObject.GetComponent<Rigidbody>();
        if (rigidBody != null)
            rigidBody.constraints = RigidbodyConstraints.None;
    }
}
