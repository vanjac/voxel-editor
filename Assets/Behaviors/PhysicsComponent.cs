using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PhysicsBehavior : EntityBehavior
{
    public static new BehaviorType objectType = new BehaviorType(
        "Physics", "Move and interact according to the laws of physics",
        "Density affects the mass of the object, proportional to its volume.",
        "soccer", typeof(PhysicsBehavior),
        BehaviorType.AndRule(
            BehaviorType.BaseTypeRule(typeof(DynamicEntity)),
            BehaviorType.NotBaseTypeRule(typeof(PlayerObject))));

    private float density = 0.5f;
    private bool gravity = true;

    public override BehaviorType BehaviorObjectType()
    {
        return objectType;
    }

    public override ICollection<Property> Properties()
    {
        return Property.JoinProperties(base.Properties(), new Property[]
        {
            new Property("den", "Density",
                () => density,
                v => density = (float)v,
                PropertyGUIs.Float),
            new Property("gra", "Gravity?",
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

// Modified from: Buoyancy.cs
// by Alex Zhdankin
// Version 2.1
//
// http://forum.unity3d.com/threads/72974-Buoyancy-script
//
// Terms of use: do whatever you like

public class PhysicsComponent : BehaviorComponent
{
    public float density;
    public bool gravity;
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
            foreach (Voxel voxel in substanceComponent.substance.voxels)
                voxels.Add(voxel.GetBounds().center - transform.position);
        else
            voxels.Add(Vector3.zero);
        base.Start();
    }

    public override void BehaviorEnabled()
    {
        SubstanceComponent sComponent = GetComponent<SubstanceComponent>();
        if (calculateVolumeAndMass)
        {
            if (sComponent != null)
                volume = sComponent.substance.voxels.Count;
            if (volume == 0)
                volume = 1;
        }
        Rigidbody rigidBody = GetComponent<Rigidbody>();
        if (rigidBody != null)
        {
            rigidBody.isKinematic = false;
            if (calculateVolumeAndMass)
                rigidBody.mass = volume * density;
            rigidBody.useGravity = gravity;
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
                float archimedesForceMagnitude = water.density * Mathf.Abs(Physics.gravity.y) * volume;
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
