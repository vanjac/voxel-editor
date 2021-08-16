using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceBehavior : EntityBehavior
{
    public enum ForceBehaviorMode
    {
        IMPULSE, CONTINUOUS
    }

    public static new BehaviorType objectType = new BehaviorType(
        "Force", "An instant or continuous force toward a target",
        "Only works for objects with a Physics behavior.\n\n"
        + "•  <b>Impulse</b> mode will cause an instant impulse to be applied when the behavior activates.\n"
        + "•  <b>Continuous</b> mode will cause the force to be continuously applied while the behavior is active.\n"
        + "•  <b>Ignore mass</b> scales the force to compensate for the mass of the object.\n"
        + "•  <b>Stop object first</b> will stop any existing motion before applying the force.",
        "rocket", typeof(ForceBehavior), BehaviorType.BaseTypeRule(typeof(DynamicEntity)));

    [EnumProp("fmo", "Mode")]
    public ForceBehaviorMode mode { get; set; } = ForceBehaviorMode.CONTINUOUS;
    [ToggleProp("ima", "Ignore mass?")]
    public bool ignoreMass { get; set; } = false;
    [ToggleProp("sto", "Stop object first?")]
    public bool stopObjectFirst { get; set; } = false;
    [FloatProp("mag", "Strength")]
    public float strength { get; set; } = 10;
    [TargetProp("dir", "Toward")]
    public Target toward { get; set; } = new Target(Target.UP);

    public override BehaviorType BehaviorObjectType()
    {
        return objectType;
    }

    public override Behaviour MakeComponent(GameObject gameObject)
    {
        var force = gameObject.AddComponent<ForceComponent>();
        if (mode == ForceBehaviorMode.IMPULSE)
        {
            if (ignoreMass)
                force.forceMode = ForceMode.VelocityChange;
            else
                force.forceMode = ForceMode.Impulse;
        }
        else if (mode == ForceBehaviorMode.CONTINUOUS)
        {
            if (ignoreMass)
                force.forceMode = ForceMode.Acceleration;
            else
                force.forceMode = ForceMode.Force;
        }
        force.stopObjectFirst = stopObjectFirst;
        force.strength = strength;
        force.toward = toward;
        return force;
    }
}

public class ForceComponent : BehaviorComponent
{
    public ForceMode forceMode;
    public float strength;
    public Target toward;
    public bool stopObjectFirst;

    private Rigidbody rigidBody;
    private NewRigidbodyController player;

    public override void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        player = GetComponent<NewRigidbodyController>();
        base.Start();
    }

    public override void BehaviorEnabled()
    {
        toward.PickRandom();
        if (stopObjectFirst && rigidBody != null)
            rigidBody.velocity = Vector3.zero;
        if ((forceMode == ForceMode.Impulse || forceMode == ForceMode.VelocityChange) && rigidBody != null)
        {
            rigidBody.AddForce(toward.DirectionFrom(transform) * strength, forceMode);
            if (player != null)
                player.disableGroundCheck = true;
        }
    }

    void FixedUpdate()
    {
        if ((forceMode == ForceMode.Force || forceMode == ForceMode.Acceleration) && rigidBody != null)
        {
            rigidBody.AddForce(toward.DirectionFrom(transform) * strength, forceMode);
            if (player != null)
                player.disableGroundCheck = true;
        }
    }
}