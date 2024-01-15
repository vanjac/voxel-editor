using System.Collections.Generic;
using UnityEngine;

public abstract class BasePhysicsBehavior : EntityBehavior
{
    public float density = 0.5f;
    public bool gravity = true;
}

public class PhysicsBehavior : BasePhysicsBehavior
{
    public static new BehaviorType objectType = new BehaviorType(
        "Physics", s => s.PhysicsDesc, s => s.PhysicsLongDesc, "soccer", typeof(PhysicsBehavior),
        BehaviorType.AndRule(
            BehaviorType.BaseTypeRule(typeof(DynamicEntity)),
            BehaviorType.NotBaseTypeRule(typeof(PlayerObject))));
    public override BehaviorType BehaviorObjectType => objectType;

    public override IEnumerable<Property> Properties() =>
        Property.JoinProperties(base.Properties(), new Property[]
        {
            new Property("den", s => s.PropDensity,
                () => density,
                v => density = (float)v,
                PropertyGUIs.Float),
            new Property("gra", s => s.PropGravity,
                () => gravity,
                v => gravity = (bool)v,
                PropertyGUIs.Toggle)
        });

    public override Behaviour MakeComponent(GameObject gameObject)
    {
        var component = gameObject.AddComponent<PhysicsComponent>();
        component.Init(this);
        return component;
    }
}

// Modified from: Buoyancy.cs
// by Alex Zhdankin
// Version 2.1
//
// http://forum.unity3d.com/threads/72974-Buoyancy-script
//
// Terms of use: do whatever you like

public class PhysicsComponent : BehaviorComponent<BasePhysicsBehavior>
{
    public float volume = 1.0f;
    public bool calculateVolumeAndMass = true;

    public bool underWater;

    private const float DAMPFER = 0.1f;
    private const float VOXEL_HALF_HEIGHT = 0.5f;

    private Vector3 localArchimedesForce;
    private List<Vector3> voxels;
    private Collider waterCollider;
    private WaterComponent water;

    public override void Start()
    {
        voxels = new List<Vector3>();
        SubstanceComponent substanceComponent = GetComponent<SubstanceComponent>();
        if (substanceComponent != null)
            foreach (Voxel voxel in substanceComponent.substance.voxelGroup.IterateVoxels())
                voxels.Add(voxel.GetBounds().center - transform.position);
        else
            voxels.Add(Vector3.zero);
        base.Start();
    }

    public override void BehaviorEnabled()
    {
        SubstanceComponent sComponent = GetComponent<SubstanceComponent>();
        if (calculateVolumeAndMass && sComponent != null)
        {
            volume = 0;
            foreach (var vc in sComponent.substance.voxelGroup.IterateComponents())
                volume += vc.voxels.Count;
            if (volume == 0)
                volume = 1;
        }
        Rigidbody rigidBody = GetComponent<Rigidbody>();
        if (rigidBody != null)
        {
            rigidBody.isKinematic = false;
            if (calculateVolumeAndMass)
                rigidBody.mass = volume * behavior.density;
            rigidBody.useGravity = behavior.gravity;
        }
    }

    public override void LastBehaviorDisabled()
    {
        Rigidbody rigidBody = GetComponent<Rigidbody>();
        if (rigidBody != null)
            rigidBody.isKinematic = true;
    }

    void OnTriggerEnter(Collider c)
    {
        WaterComponent cWater = c.GetComponent<WaterComponent>();
        if (cWater == null && c.transform.parent != null)
            cWater = c.transform.parent.GetComponent<WaterComponent>();
        if (cWater != null)
        {
            waterCollider = c;
            if (water != cWater)
            {
                water = cWater;
                float archimedesForceMagnitude = water.Density * Mathf.Abs(Physics.gravity.y) * volume;
                localArchimedesForce = new Vector3(0, archimedesForceMagnitude, 0) / voxels.Count;
            }
        }
    }

    void OnTriggerExit(Collider c)
    {
        if (c == waterCollider)
        {
            water = null;
            waterCollider = null;
        }
    }

    void OnCollisionEnter(Collision c)
    {
        OnTriggerEnter(c.collider);
    }

    void OnCollisionExit(Collision c)
    {
        OnTriggerExit(c.collider);
    }

    private float GetWaterLevel(float x, float z)
    {
        if (water != null && water.enabled)
            return water.GetWaterLevel(x, z);
        return float.MinValue;
    }

    void FixedUpdate()
    {
        underWater = false;
        foreach (var point in voxels)
        {
            var wp = transform.TransformPoint(point);
            float waterLevel = GetWaterLevel(wp.x, wp.z);

            if (wp.y - VOXEL_HALF_HEIGHT < waterLevel)
            {
                underWater = true;
                float k = (waterLevel - wp.y) / (2 * VOXEL_HALF_HEIGHT) + 0.5f;
                if (k > 1)
                {
                    k = 1f;
                }
                else if (k < 0)
                {
                    k = 0f;
                }

                var velocity = GetComponent<Rigidbody>().GetPointVelocity(wp);
                var localDampingForce = -velocity * DAMPFER * GetComponent<Rigidbody>().mass;
                var force = localDampingForce + Mathf.Sqrt(k) * localArchimedesForce;
                GetComponent<Rigidbody>().AddForceAtPosition(force, wp);
            }
        }
    }
}
