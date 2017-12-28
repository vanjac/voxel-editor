using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Substance : DynamicEntity
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Substance", "An object made of blocks", "cube-outline", typeof(Substance));

    public HashSet<Voxel> voxels;

    private VoxelArray voxelArray;

    public Substance(VoxelArray array)
    {
        voxels = new HashSet<Voxel>();
        voxelArray = array;
        if (!Voxel.InEditor())
        {
            GameObject substanceObject = new GameObject();
            substanceObject.transform.parent = voxelArray.transform;
            SubstanceComponent component = substanceObject.AddComponent<SubstanceComponent>();
            component.entity = this;
            component.substance = this;
            this.component = component;
        }
    }

    public override PropertiesObjectType ObjectType()
    {
        return objectType;
    }

    // called by voxel
    public void AddVoxel(Voxel v)
    {
        voxels.Add(v);
        if (component != null)
            v.transform.parent = component.transform;
    }

    public void RemoveVoxel(Voxel v)
    {
        voxels.Remove(v);
        if (component != null)
            v.transform.parent = component.transform.parent;
    }

    public override void UpdateEntity()
    {
        base.UpdateEntity();
        foreach (Voxel v in voxels)
            v.UpdateVoxel();
    }
}

public class SubstanceComponent : EntityComponent
{
    public Substance substance;

    public override void Start()
    {
        base.Start();

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
    }
}