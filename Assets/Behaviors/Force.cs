using System.Collections.Generic;
using UnityEngine;

public class ForceBehavior : GenericEntityBehavior<ForceBehavior, ForceComponent>
{
    public enum ForceBehaviorMode
    {
        IMPULSE, CONTINUOUS
    }

    public static new BehaviorType objectType = new BehaviorType(
        "Force", s => s.ForceDesc, s => s.ForceLongDesc, "rocket", typeof(ForceBehavior),
        BehaviorType.BaseTypeRule(typeof(DynamicEntity)));
    public override BehaviorType BehaviorObjectType => objectType;

    public ForceBehaviorMode mode = ForceBehaviorMode.CONTINUOUS;
    public bool ignoreMass = false;
    public bool stopObjectFirst = false;
    public float strength = 10;
    public Target target = new Target(Target.UP);

    public override IEnumerable<Property> Properties() =>
        Property.JoinProperties(base.Properties(), new Property[]
        {
            new Property("fmo", s => s.PropMode,
                () => mode,
                v => mode = (ForceBehaviorMode)v,
                PropertyGUIs.Enum),
            new Property("ima", s => s.PropIgnoreMass,
                () => ignoreMass,
                v => ignoreMass = (bool)v,
                PropertyGUIs.Toggle),
            new Property("sto", s => s.PropStopObjectFirst,
                () => stopObjectFirst,
                v => stopObjectFirst = (bool)v,
                PropertyGUIs.Toggle),
            new Property("mag", s => s.PropStrength,
                () => strength,
                v => strength = (float)v,
                PropertyGUIs.Float),
            new Property("dir", s => s.PropToward,
                () => target,
                v => target = (Target)v,
                PropertyGUIs.Target)
        });
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