using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsBehavior : EntityBehavior
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Physics", "Move and interact according to the laws of physics", "soccer",
        typeof(PhysicsBehavior));

    private float mass = 1.0f;
    private bool gravity = true;

    public override PropertiesObjectType ObjectType()
    {
        return objectType;
    }

    public override ICollection<Property> Properties()
    {
        return Property.JoinProperties(base.Properties(), new Property[]
        {
            new Property("Mass",
                () => mass,
                v => mass = (float)v,
                PropertyGUIs.Float),
            new Property("Gravity?",
                () => gravity,
                v => gravity = (bool)v,
                PropertyGUIs.Toggle)
        });
    }

    public override Behaviour MakeComponent(GameObject gameObject)
    {
        PhysicsComponent component = gameObject.AddComponent<PhysicsComponent>();
        component.mass = mass;
        component.gravity = gravity;
        return component;
    }
}

public class PhysicsComponent : MonoBehaviour
{
    public float mass;
    public bool gravity;

    void Start()
    {
        if (enabled)
            OnEnable();
        else
            OnDisable();
    }

    void OnEnable()
    {
        Rigidbody rigidBody = gameObject.GetComponent<Rigidbody>();
        if (rigidBody != null)
        {
            rigidBody.isKinematic = false;
            rigidBody.mass = mass;
            rigidBody.useGravity = gravity;
        }
    }

    void OnDisable()
    {
        Rigidbody rigidBody = gameObject.GetComponent<Rigidbody>();
        if (rigidBody != null)
            rigidBody.isKinematic = true;
    }
}
