using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveBehavior : EntityBehavior
{
    public static new BehaviorType objectType = new BehaviorType(
        "Move", "Move in a direction or toward an object",
        "If both Solid and Physics behaviors are active, object will not be able to pass through other objects. "
        + "Increase the Density property of the Physics behavior to increase the object's pushing strength. Gravity has no effect while Move is active.",
        "arrow-right-bold-box-outline", typeof(MoveBehavior), BehaviorType.BaseTypeRule(typeof(DynamicEntity)));

    private Target target = new Target(0);
    private float speed = 1;

    public override BehaviorType BehaviorObjectType()
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

public class MoveComponent : BehaviorComponent
{
    public Target target;
    public float speed;

    private Rigidbody rigidBody;

    public override void Start()
    {
        rigidBody = gameObject.GetComponent<Rigidbody>();
        base.Start();
    }

    void OnCollisionEnter()
    {
        if (rigidBody != null)
            rigidBody.velocity = Vector3.zero;
    }

    public override void BehaviorDisabled()
    {
        if (rigidBody != null)
            rigidBody.constraints = RigidbodyConstraints.None;
    }

    void FixedUpdate()
    {
        if (rigidBody != null)
            rigidBody.velocity = Vector3.zero;
        Vector3 move = GetMoveFixed();
        if (rigidBody != null)
        {
            var constraints = RigidbodyConstraints.FreezeRotation;
            if (move.x == 0)
                constraints |= RigidbodyConstraints.FreezePositionX;
            if (move.y == 0)
                constraints |= RigidbodyConstraints.FreezePositionY;
            if (move.z == 0)
                constraints |= RigidbodyConstraints.FreezePositionZ;
            rigidBody.constraints = constraints;
            rigidBody.MovePosition(rigidBody.position + move);
        }
        else
            transform.Translate(move);

    }

    public Vector3 GetMoveFixed()
    {
        return target.DirectionFrom(transform.position) * speed * Time.fixedDeltaTime;
    }
}
