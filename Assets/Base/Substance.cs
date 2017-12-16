using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Substance : DynamicEntity
{
    private HashSet<Voxel> voxels;

    public bool visible = true;
    public bool solid = true;

    public Substance()
    {
        voxels = new HashSet<Voxel>();
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
}