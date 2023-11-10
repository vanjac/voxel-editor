using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceBehavior : GenericEntityBehavior<ForceBehavior, ForceComponent>
{
    public enum ForceBehaviorMode
    {
        IMPULSE, CONTINUOUS
    }

    public static new BehaviorType objectType = new BehaviorType(
        "Force", "Apply instant or continuous force",
        "Only works for objects with a Physics behavior.\n\n"
        + "•  <b>Impulse</b> mode will cause an instant impulse to be applied when the behavior activates.\n"
        + "•  <b>Continuous</b> mode will cause the force to be continuously applied while the behavior is active.\n"
        + "•  <b>Ignore mass</b> scales the force to compensate for the mass of the object.\n"
        + "•  <b>Stop object first</b> will stop any existing motion before applying the force.",
        "rocket", typeof(ForceBehavior), BehaviorType.BaseTypeRule(typeof(DynamicEntity)));
    public override BehaviorType BehaviorObjectType => objectType;

    public ForceBehaviorMode mode = ForceBehaviorMode.CONTINUOUS;
    public bool ignoreMass = false;
    public bool stopObjectFirst = false;
    public float strength = 10;
    public Target target = new Target(Target.UP);

    public override ICollection<Property> Properties()
    {
        return Property.JoinProperties(base.Properties(), new Property[]
        {
            new Property("fmo", "Mode",
                () => mode,
                v => mode = (ForceBehaviorMode)v,
                PropertyGUIs.Enum),
            new Property("ima", "Ignore mass?",
                () => ignoreMass,
                v => ignoreMass = (bool)v,
                PropertyGUIs.Toggle),
            new Property("sto", "Stop object first?",
                () => stopObjectFirst,
                v => stopObjectFirst = (bool)v,
                PropertyGUIs.Toggle),
            new Property("mag", "Strength",
                () => strength,
                v => strength = (float)v,
                PropertyGUIs.Float),
            new Property("dir", "Toward",
                () => target,
                v => target = (Target)v,
                PropertyGUIs.Target)
        });
    }
}

public class ForceComponent : BehaviorComponent<ForceBehavior>
{
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
        behavior.target.PickRandom();
        if (behavior.stopObjectFirst && rigidBody != null)
            rigidBody.velocity = Vector3.zero;
        if (behavior.mode == ForceBehavior.ForceBehaviorMode.IMPULSE && rigidBody != null)
        {
            ForceMode mode = behavior.ignoreMass ? ForceMode.VelocityChange : ForceMode.Impulse;
            rigidBody.AddForce(behavior.target.DirectionFrom(transform) * behavior.strength, mode);
            if (player != null)
                player.disableGroundCheck = true;
        }
    }

    void FixedUpdate()
    {
        if (behavior.mode == ForceBehavior.ForceBehaviorMode.CONTINUOUS && rigidBody != null)
        {
            ForceMode mode = behavior.ignoreMass ? ForceMode.Acceleration : ForceMode.Force;
            rigidBody.AddForce(behavior.target.DirectionFrom(transform) * behavior.strength, mode);
            if (player != null)
                player.disableGroundCheck = true;
        }
    }
}