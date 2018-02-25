using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Substance : DynamicEntity
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Substance", "An entity made of blocks", "cube-outline", typeof(Substance));

    public HashSet<Voxel> voxels;

    private VoxelArray voxelArray;

    public Substance(VoxelArray array)
    {
        voxels = new HashSet<Voxel>();
        voxelArray = array;
    }

    public override PropertiesObjectType ObjectType()
    {
        return objectType;
    }

    public override void InitEntityGameObject()
    {
        GameObject substanceObject = new GameObject();
        substanceObject.transform.parent = voxelArray.transform;
        SubstanceComponent component = substanceObject.AddComponent<SubstanceComponent>();
        component.entity = this;
        component.substance = this;
        this.component = component;
    }

    // called by voxel
    public void AddVoxel(Voxel v)
    {
        voxels.Add(v);
    }

    public void RemoveVoxel(Voxel v)
    {
        voxels.Remove(v);
    }

    public override void UpdateEntity()
    {
        base.UpdateEntity();
        foreach (Voxel v in voxels)
            v.UpdateVoxel();
    }

    public Vector3 CalculateCenterPoint()
    {
        Bounds voxelBounds = new Bounds();
        foreach (Voxel voxel in voxels)
        {
            if (voxelBounds.extents == Vector3.zero)
                voxelBounds = voxel.GetBounds();
            else
                voxelBounds.Encapsulate(voxel.GetBounds());
        }
        return voxelBounds.center;
    }
}

public class SubstanceComponent : EntityComponent
{
    public Substance substance;

    public override void Start()
    {
        foreach (Voxel voxel in substance.voxels)
            voxel.transform.parent = transform;
        Vector3 centerPoint = substance.CalculateCenterPoint();
        transform.position = centerPoint;
        foreach (Voxel voxel in substance.voxels)
            voxel.transform.position -= centerPoint;

        // a rigidBody is required for collision detection
        Rigidbody rigidBody = gameObject.AddComponent<Rigidbody>();
        // no physics by default (could be disabled by a Physics behavior)
        rigidBody.isKinematic = true;

        base.Start();
    }
}