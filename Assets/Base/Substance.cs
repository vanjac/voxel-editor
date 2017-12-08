using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Substance : DynamicEntity
{
    private HashSet<Voxel> voxels;

    public Substance()
    {
        voxels = new HashSet<Voxel>();
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