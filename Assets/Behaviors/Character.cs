using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterBehavior : PhysicsBehavior
{
    public static new BehaviorType objectType = new BehaviorType(
        "Character",
        "Move with character physics, including gravity.",
        "human", typeof(CharacterBehavior),
        BehaviorType.AndRule(
            BehaviorType.BaseTypeRule(typeof(DynamicEntity)),
            BehaviorType.NotBaseTypeRule(typeof(PlayerObject))));

    public override BehaviorType BehaviorObjectType()
    {
        return objectType;
    }

    public override ICollection<Property> Properties()
    {
        var properties = new List<Property>();
        foreach (var p in base.Properties())
        {
            if (p.id != "gra")
                properties.Add(p);
        }
        return properties;
    }

    public override Behaviour MakeComponent(GameObject gameObject)
    {
        var component = gameObject.AddComponent<CharacterComponent>();
        component.density = density;
        component.gravity = true;
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
