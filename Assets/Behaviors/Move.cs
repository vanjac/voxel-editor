using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveBehavior : EntityBehavior
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Move", "Move in a direction or toward an object",
        "arrow-right-bold-box-outline", typeof(MoveBehavior));

    private Target target = new Target(0);
    private float speed = 1;

    public override PropertiesObjectType ObjectType()
    {
        return objectType;
    }

    public override ICollection<Property> Properties()
    {
        return Property.JoinProperties(base.Properties(), new Property[]
        {
            new Property("Speed",
                () => speed,
                v => speed = (float)v,
                PropertyGUIs.Float),
            new Property("Toward",
                () => target,
                v => target = (Target)v,
                PropertyGUIs.Target)
        });
    }

    public override Behaviour MakeComponent(GameObject gameObject)
    {
        MoveComponent move = gameObject.AddComponent<MoveComponent>();
        move.target = target;
        move.speed = speed;
        return move;
    }
}

public class MoveComponent : MonoBehaviour
{
    public Target target;
    public float speed;

    private Rigidbody rigidBody;

    void Start()
    {
        rigidBody = gameObject.GetComponent<Rigidbody>();
    }

    void OnCollisionEnter()
    {
        rigidBody.velocity = Vector3.zero;
    }

    void OnEnable()
    {
        Rigidbody rigidBody = gameObject.GetComponent<Rigidbody>();
        if (rigidBody != null)
            rigidBody.constraints = RigidbodyConstraints.FreezeRotation;
    }

    void OnDisable()
    {
        Rigidbody rigidBody = gameObject.GetComponent<Rigidbody>();
        if (rigidBody != null)
            rigidBody.constraints = RigidbodyConstraints.None;
    }

    void FixedUpdate()
    {
        rigidBody.velocity = Vector3.zero;
        Vector3 move = target.directionFrom(transform.position) * speed * Time.fixedDeltaTime;
        if (rigidBody != null)
            rigidBody.MovePosition(rigidBody.position + move);
        else
            transform.Translate(move);
    }
}
