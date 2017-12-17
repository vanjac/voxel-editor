using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Substance : DynamicEntity
{
    public HashSet<Voxel> voxels;

    public bool visible = true;
    public bool solid = true;

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

    public override ICollection<Property> Properties()
    {
        List<Property> props = new List<Property>(base.Properties());
        props.AddRange(new Property[]
        {
            new Property("Visible?",
                () => visible,
                v => visible = (bool)v,
                PropertyGUIs.Toggle),
            new Property("Solid?",
                () => solid,
                v => solid = (bool)v,
                PropertyGUIs.Toggle)
        });
        return props;
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
        foreach (EntityBehavior behavior in substance.behaviors)
        {
            behavior.MakeComponent(gameObject);
        }
    }
}