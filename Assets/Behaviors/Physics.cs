using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsBehavior : EntityBehavior
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Physics", "Move and interact according to the laws of physics", "soccer",
        typeof(PhysicsBehavior));

    private float density = 0.5f;
    private bool gravity = true;

    public override PropertiesObjectType ObjectType()
    {
        return objectType;
    }

    public override ICollection<Property> Properties()
    {
        return Property.JoinProperties(base.Properties(), new Property[]
        {
            new Property("Density",
                () => density,
                v => density = (float)v,
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
        component.density = density;
        component.gravity = gravity;
        return component;
    }
}

public class PhysicsComponent : MonoBehaviour
{
    public float density;
    public bool gravity;
    public float volume = 1.0f;

    void Start()
    {
        if (enabled)
            OnEnable();
        else
            OnDisable();
    }

    void OnEnable()
    {
        SubstanceComponent sComponent = GetComponent<SubstanceComponent>();
        if (sComponent != null)
            volume = sComponent.substance.voxels.Count;
        if (volume == 0)
            volume = 1;
        Rigidbody rigidBody = GetComponent<Rigidbody>();
        if (rigidBody != null)
        {
            rigidBody.isKinematic = false;
            rigidBody.mass = volume * density;
            rigidBody.useGravity = gravity;
        }
    }

    void OnDisable()
    {
        Rigidbody rigidBody = GetComponent<Rigidbody>();
        if (rigidBody != null)
            rigidBody.isKinematic = true;
    }
}
