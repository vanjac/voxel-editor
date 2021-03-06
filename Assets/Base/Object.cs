﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ObjectEntity : DynamicEntity
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Object", "An object not made of blocks", "circle", typeof(ObjectEntity));

    public ObjectMarker marker;
    public Vector3Int position;
    public float rotation;
    public VoxelFace paint;
    public Color highlight = Color.clear;
    public Material highlightMaterial;

    public override PropertiesObjectType ObjectType()
    {
        return objectType;
    }

    public virtual Vector3 PositionOffset()
    {
        return Vector3.zero;
    }

    public override void UpdateEntityEditor()
    {
        if (marker != null)
            marker.UpdateMarker();
    }

    public override Vector3 PositionInEditor()
    {
        return position + new Vector3(0.5f, 0.5f, 0.5f) + PositionOffset();
    }

    public override bool AliveInEditor()
    {
        return marker != null;
    }

    public override void SetHighlight(Color c)
    {
        if (c == highlight)
            return;
        highlight = c;
        if (highlightMaterial == null)
            highlightMaterial = ResourcesDirectory.InstantiateMaterial(
                ResourcesDirectory.FindMaterial("UNLIT", true));
        highlightMaterial.color = highlight;
        if (marker != null)
            marker.UpdateMarker();
    }

    public void InitObjectMarker(VoxelArrayEditor voxelArray)
    {
        marker = CreateObjectMarker(voxelArray);
        marker.transform.parent = voxelArray.transform;
        marker.objectEntity = this;
        marker.tag = "ObjectMarker";
    }

    public override EntityComponent InitEntityGameObject(VoxelArray voxelArray, bool storeComponent = true)
    {
        var c = CreateEntityComponent(voxelArray);
        c.transform.parent = voxelArray.transform;
        c.transform.position = PositionInEditor();
        c.transform.rotation = Quaternion.Euler(new Vector3(0, rotation, 0));
        c.entity = this;
        c.health = health;
        if (storeComponent)
            component = c;
        Renderer renderer = c.GetComponent<Renderer>();
        if (renderer != null)
        {
            List<Material> materials = new List<Material>();
            if (paint.material != null)
                materials.Add(paint.material);
            if (paint.overlay != null)
                materials.Add(paint.overlay);
            renderer.materials = materials.ToArray();
        }
        return c;
    }

    protected abstract ObjectMarker CreateObjectMarker(VoxelArrayEditor voxelArray);
    protected abstract DynamicEntityComponent CreateEntityComponent(VoxelArray voxelArray);
}