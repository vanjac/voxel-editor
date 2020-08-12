using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarryableBehavior : EntityBehavior
{
    public static new BehaviorType objectType = new BehaviorType(
        "Carryable", "Allow the player to pick up and drop/throw the object",
        "Tap to pick up the object, tap again to throw."
        + " Increasing the throw angle causes the object to be thrown with an arc.\n\n"
        + "Requires physics",
        "coffee", typeof(CarryableBehavior),
        BehaviorType.AndRule(
            BehaviorType.BaseTypeRule(typeof(DynamicEntity)),
            BehaviorType.NotBaseTypeRule(typeof(PlayerObject))));
    
    private float throwSpeed = 0;
    private float throwAngle = 25;

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
    // measured from player feet to point on object closest to player feet
    private static readonly Vector3 CARRY_VECTOR = new Vector3(0, 0.1f, 0.85f);
    private const float MASS_SCALE = 400f;  // higher values have less effect on player physics
    private const float BREAK_FORCE = 40f;
    private const float PICK_UP_TIME = 0.25f;

    public float throwSpeed, throwAngle;
    private FixedJoint joint;
    private Rigidbody rb;

    public override void Start()
    {
        rb = GetComponent<Rigidbody>();
        base.Start();
    }

    public bool IsCarried()
    {
        return joint != null;
    }

    public void Carry(EntityComponent player)
    {
        joint = gameObject.AddComponent<FixedJoint>();
        joint.connectedBody = player.GetComponent<Rigidbody>();
        joint.massScale = MASS_SCALE * rb.mass;
        joint.breakForce = BREAK_FORCE;
        StartCoroutine(PickUpAnimCoroutine(player));
    }

    public void Throw(EntityComponent player)
    {
        Drop();
        if (throwSpeed != 0 && rb != null)
        {
            float degrees = throwAngle * Mathf.Deg2Rad;
            Vector3 throwNormal = player.transform.forward * Mathf.Cos(degrees)
                + Vector3.up * Mathf.Sin(degrees);
            rb.AddForce(throwNormal * throwSpeed, ForceMode.VelocityChange);
        }
    }

    public void Drop()
    {
        if (joint == null)
            return;
        Destroy(joint);
        joint = null;
        StartCoroutine(WakeUpCoroutine());
    }

    private IEnumerator WakeUpCoroutine()
    {
        if (rb == null)
            yield break;
        // please wake up
        rb.WakeUp();
        yield return new WaitForFixedUpdate();
        rb.WakeUp();
    }

    private IEnumerator PickUpAnimCoroutine(EntityComponent player)
    {
        joint.enableCollision = false;
        // calculate the start anchor...
        joint.autoConfigureConnectedAnchor = true;
        yield return new WaitForFixedUpdate();
        if (joint == null)
            yield break;
        Vector3 startAnchor = joint.connectedAnchor;
        joint.autoConfigureConnectedAnchor = false;

        Vector3 carryVector = CARRY_VECTOR;
        carryVector += Vector3.down * player.GetComponent<CapsuleCollider>().height / 2;
        Bounds bounds = GetRigidbodyBounds(rb);
        carryVector += Vector3.up * (rb.transform.position.y - bounds.min.y);
        // get closest point when object is moved in front of player
        Vector3 carryPoint = rb.ClosestPointOnBounds(
            rb.transform.position - player.transform.forward * 100f);
        float carryDist = Vector3.Project(carryPoint - rb.transform.position, player.transform.forward).magnitude;
        carryVector += Vector3.forward * carryDist;

        float startTime = Time.fixedTime;
        while (Time.fixedTime - startTime < PICK_UP_TIME)
        {
            joint.connectedAnchor = Vector3.Slerp(startAnchor, carryVector,
                EaseInOutSine((Time.fixedTime - startTime) / PICK_UP_TIME));
            yield return new WaitForFixedUpdate();
            if (joint == null)
                yield break;
        }
        joint.connectedAnchor = carryVector;
        joint.enableCollision = true;
    }

    float EaseInOutSine(float x)
    {
        // https://easings.net/#easeInOutSine
        return -(Mathf.Cos(Mathf.PI * x) - 1) / 2;
    }

    private Bounds GetRigidbodyBounds(Rigidbody rb)
    {
        Collider[] colliders = rb.GetComponentsInChildren<Collider>();
        Bounds b = colliders[0].bounds;
        foreach (Collider c in colliders)
            b.Encapsulate(c.bounds);
        return b;
    }

    public override void BehaviorDisabled()
    {
        Drop();
    }
}