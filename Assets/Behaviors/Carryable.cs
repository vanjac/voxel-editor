using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarryableBehavior : EntityBehavior
{
    public static new BehaviorType objectType = new BehaviorType(
        "Carryable", "Allow the player to pick up and drop/throw the object",
        "coffee", typeof(CarryableBehavior),
        BehaviorType.AndRule(
            BehaviorType.BaseTypeRule(typeof(DynamicEntity)),
            BehaviorType.NotBaseTypeRule(typeof(PlayerObject))));
    
    private float throwSpeed = 0;
    private float throwAngle = 0;

    public override BehaviorType BehaviorObjectType()
    {
        return objectType;
    }

    public override ICollection<Property> Properties()
    {
        return Property.JoinProperties(base.Properties(), new Property[]
        {
            new Property("ths", "Throw speed",
                () => throwSpeed,
                v => throwSpeed = (float)v,
                PropertyGUIs.Float),
            new Property("tha", "Throw angle",
                () => throwAngle,
                v => throwAngle = (float)v,
                PropertyGUIs.Float),
        });
    }

    public override Behaviour MakeComponent(GameObject gameObject)
    {
        var component = gameObject.AddComponent<CarryableComponent>();
        component.throwSpeed = throwSpeed;
        component.throwAngle = throwAngle;
        return component;
    }
}


public class CarryableComponent : BehaviorComponent
{
    private static readonly Vector3 CARRY_VECTOR = new Vector3(0, -0.4f, 1.5f);
    private const float MASS_SCALE = 400f;  // higher values have less effect on player physics
    private const float BREAK_FORCE = 40f;

    public float throwSpeed, throwAngle;
    private FixedJoint joint;

    public void Tap(EntityComponent player)
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
            return;
        if (joint == null)
        {
            joint = gameObject.AddComponent<FixedJoint>();
            joint.connectedBody = player.GetComponent<Rigidbody>();
            joint.massScale = MASS_SCALE * rb.mass;
            joint.breakForce = BREAK_FORCE;
            joint.autoConfigureConnectedAnchor = false;
            joint.connectedAnchor = CARRY_VECTOR;
        }
        else
        {
            StartCoroutine(Drop());
            float degrees = throwAngle * Mathf.Deg2Rad;
            Vector3 throwNormal = player.transform.forward * Mathf.Cos(degrees)
                + Vector3.up * Mathf.Sin(degrees);
            rb.AddForce(throwNormal * throwSpeed, ForceMode.VelocityChange);
        }
    }

    private IEnumerator Drop()
    {
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb == null)
            yield break;
        Destroy(joint);
        joint = null;
        rb.WakeUp();
        yield return null;
        rb.WakeUp();
    }

    public override void BehaviorDisabled()
    {
        if (joint != null)
            StartCoroutine(Drop());
    }
}