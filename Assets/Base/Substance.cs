using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Substance : DynamicEntity
{
    public HashSet<Voxel> voxels;

    private VoxelArray voxelArray;
    public GameObject substanceObject;

    public Substance(VoxelArray array)
    {
        voxels = new HashSet<Voxel>();
        voxelArray = array;
        if (!Voxel.InEditor())
        {
            substanceObject = new GameObject();
            substanceObject.transform.parent = voxelArray.transform;
            SubstanceComponent component = substanceObject.AddComponent<SubstanceComponent>();
            component.substance = this;
        }
    }

    public override string TypeName()
    {
        return "Substance";
    }

    // called by voxel
    public void AddVoxel(Voxel v)
    {
        voxels.Add(v);
        if (substanceObject != null)
            v.transform.parent = substanceObject.transform;
    }

    public void RemoveVoxel(Voxel v)
    {
        voxels.Remove(v);
        if (substanceObject != null)
            v.transform.parent = substanceObject.transform.parent;
    }

    public override void UpdateEntity()
    {
        base.UpdateEntity();
        foreach (Voxel v in voxels)
            v.UpdateVoxel();
    }
}

public class SubstanceComponent : MonoBehaviour
{
    public Substance substance;

    private List<Behaviour> offComponents = new List<Behaviour>();
    private List<Behaviour> onComponents = new List<Behaviour>();

    private SensorComponent sensorComponent;
    private bool sensorWasOn;

    void Start()
    {
        Bounds voxelBounds = new Bounds();
        foreach (Voxel voxel in substance.voxels)
            if (voxelBounds.extents == Vector3.zero)
                voxelBounds = voxel.GetBounds();
            else
                voxelBounds.Encapsulate(voxel.GetBounds());
        Vector3 centerPoint = voxelBounds.center;
        transform.position = centerPoint;
        foreach (Voxel voxel in substance.voxels)
            voxel.transform.position -= centerPoint;

        // a rigidBody is required for collision detection
        Rigidbody rigidBody = gameObject.AddComponent<Rigidbody>();
        rigidBody.isKinematic = true; // no physics by default

        if (substance.sensor != null)
            sensorComponent = substance.sensor.MakeComponent(gameObject);
        sensorWasOn = false;
        foreach (EntityBehavior behavior in substance.behaviors)
        {
            Behaviour c = behavior.MakeComponent(gameObject);
            if (behavior.condition == EntityBehavior.Condition.OFF)
                offComponents.Add(c);
            else if (behavior.condition == EntityBehavior.Condition.ON)
            {
                onComponents.Add(c);
                c.enabled = false;
            }
        }
    }

    void Update()
    {
        if (sensorComponent == null)
            return;
        bool sensorIsOn = sensorComponent.isOn();
        if (sensorIsOn && !sensorWasOn)
        {
            foreach (Behaviour onComponent in onComponents)
                onComponent.enabled = true;
            foreach (Behaviour offComponent in offComponents)
                offComponent.enabled = false;
        }
        else if (!sensorIsOn && sensorWasOn)
        {
            foreach (Behaviour onComponent in onComponents)
                onComponent.enabled = false;
            foreach (Behaviour offComponent in offComponents)
                offComponent.enabled = true;
        }
        sensorWasOn = sensorIsOn;
    }
}