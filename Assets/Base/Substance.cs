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
    private enum SensorCycle
    {
        OFF, TURNING_ON, ON, TURNING_OFF, TIMED_OUT
    }

    private static bool SensorOn(SensorCycle cycle)
    {
        return cycle == SensorCycle.TURNING_ON
            || cycle == SensorCycle.ON
            || cycle == SensorCycle.TIMED_OUT;
    }

    private static bool BehaviorsOn(SensorCycle cycle)
    {
        return cycle == SensorCycle.ON || cycle == SensorCycle.TURNING_OFF;
    }

    public Substance substance;

    private List<Behaviour> offComponents = new List<Behaviour>();
    private List<Behaviour> onComponents = new List<Behaviour>();

    private SensorComponent sensorComponent;
    private SensorCycle _sensorCycle;
    private SensorCycle sensorCycle
    {
        get
        {
            return _sensorCycle;
        }
        set
        {
            if (SensorOn(value) && !SensorOn(_sensorCycle))
                t_sensorOn = Time.time;
            if (BehaviorsOn(value) && !BehaviorsOn(_sensorCycle))
            {
                SetBehaviors(true);
                t_behaviorsOn = Time.time;
            }
            else if(!BehaviorsOn(value) && BehaviorsOn(_sensorCycle))
                SetBehaviors(false);
            _sensorCycle = value;
        }
    }
    private float t_behaviorsOn;
    private float t_sensorOn;


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
        sensorCycle = SensorCycle.OFF;
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
        bool sensorIsOn = sensorComponent.isOn() ^ substance.sensor.invert;

        // change cycle state based on sensor
        switch (sensorCycle)
        {
            case SensorCycle.OFF:
                if (sensorIsOn)
                    sensorCycle = SensorCycle.TURNING_ON;
                break;
            case SensorCycle.TURNING_ON:
                if (!sensorIsOn)
                    sensorCycle = SensorCycle.OFF;
                break;
            case SensorCycle.ON:
                if (!sensorIsOn)
                    sensorCycle = SensorCycle.TURNING_OFF;
                break;
            case SensorCycle.TURNING_OFF:
                if (sensorIsOn)
                    sensorCycle = SensorCycle.ON;
                break;
            case SensorCycle.TIMED_OUT:
                if (!sensorIsOn)
                    sensorCycle = SensorCycle.OFF;
                break;
        }

        float time = Time.time;

        // change cycle state based on time
        switch (sensorCycle)
        {
            case SensorCycle.TURNING_ON:
                if (time - t_sensorOn > substance.sensor.turnOnTime)
                    sensorCycle = SensorCycle.ON;
                break;
            case SensorCycle.ON:
                if (time - t_behaviorsOn > substance.sensor.maxOnTime)
                    sensorCycle = SensorCycle.TIMED_OUT;
                break;
            case SensorCycle.TURNING_OFF:
                // timeSinceChange is time since ON, not time since TURNING_OFF
                if (time - t_behaviorsOn > substance.sensor.minOnTime)
                    sensorCycle = SensorCycle.OFF;
                break;
        }
    } // Update()

    private void SetBehaviors(bool on)
    {
        foreach (Behaviour onComponent in onComponents)
            onComponent.enabled = on;
        foreach (Behaviour offComponent in offComponents)
            offComponent.enabled = !on;
    }
}