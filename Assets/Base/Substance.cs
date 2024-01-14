using System.Collections.Generic;
using UnityEngine;

public struct Pivot
{
    public enum Pos { Min, Center, Max };
    public Pos x, y, z;
}

public class Substance : DynamicEntity
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Substance", s => s.SubstanceDesc, s => s.SubstanceLongDesc, "cube-outline",
        typeof(Substance));
    public override PropertiesObjectType ObjectType => objectType;

    public Pivot pivot = new Pivot { x = Pivot.Pos.Center, y = Pivot.Pos.Center, z = Pivot.Pos.Center };

    public VoxelGroup voxelGroup = new VoxelGroup();

    public Color highlight = Color.clear;
    public Material highlightMaterial;
    public VoxelFace defaultPaint;

    public override IEnumerable<Property> Properties() =>
        Property.JoinProperties(base.Properties(), new Property[]
        {
            new Property("piv", "Pivot",
                () => pivot,
                v => pivot = (Pivot)v,
                PropertyGUIs.PivotProp),
        });

    public override EntityComponent InitEntityGameObject(VoxelArray voxelArray, bool storeComponent = true)
    {
        GameObject substanceObject = new GameObject();
        substanceObject.name = "Substance";
        substanceObject.transform.parent = voxelArray.transform;
        substanceObject.transform.position = PositionInEditor();

        foreach (VoxelComponent vc in voxelGroup.IterateComponents())
        {
            // TODO: need to update this!
            if (storeComponent)
            {
                vc.transform.parent = substanceObject.transform;
            }
            else
            {
                // clone
                VoxelComponent vClone = vc.Clone();
                vClone.transform.parent = substanceObject.transform;
                vClone.transform.position = vc.transform.position;
                vClone.transform.rotation = vc.transform.rotation;
            }
        }
        SubstanceComponent component = substanceObject.AddComponent<SubstanceComponent>();
        component.entity = this;
        component.substance = this;
        component.health = health;
        if (storeComponent)
            this.component = component;
        return component;
    }

    public override bool AliveInEditor()
    {
        foreach (var v in voxelGroup.IterateVoxels())
            return true;
        return false;
    }

    public override void UpdateEntityEditor()
    {
        base.UpdateEntityEditor();
        foreach (VoxelComponent v in voxelGroup.IterateComponents())
            v.UpdateVoxel();
    }

    public override Vector3 PositionInEditor()
    {
        Bounds voxelBounds = new Bounds();
        foreach (Voxel voxel in voxelGroup.IterateVoxels())
        {
            if (voxelBounds.extents == Vector3.zero)
                voxelBounds = voxel.GetBounds();
            else
                voxelBounds.Encapsulate(voxel.GetBounds());
        }
        var factor = new Vector3((float)pivot.x, (float)pivot.y, (float)pivot.z) / 2;
        return voxelBounds.min + Vector3.Scale(voxelBounds.size, factor);
    }

    public override void SetHighlight(Color c)
    {
        if (c == highlight)
            return;
        highlight = c;
        if (highlightMaterial == null)
            highlightMaterial = ResourcesDirectory.InstantiateMaterial(VoxelComponent.highlightMaterials[15]);
        highlightMaterial.color = highlight;
        foreach (VoxelComponent v in voxelGroup.IterateComponents())
            v.UpdateVoxel();
    }
}

public class SubstanceComponent : DynamicEntityComponent
{
    public Substance substance;

    public override void Start()
    {
        // a rigidBody is required for collision detection
        Rigidbody rigidBody = gameObject.AddComponent<Rigidbody>(); // TODO slow for large number of colliders!
        // no physics by default (could be disabled by a Physics behavior)
        rigidBody.isKinematic = true;

        base.Start();
    }
}
