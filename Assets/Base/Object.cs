﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ObjectEntity : DynamicEntity
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Object", "An object not made of blocks", "circle", typeof(ObjectEntity));

    public ObjectMarker marker;
    public Vector3Int position;

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
        c.entity = this;
        c.health = health;
        if (storeComponent)
            component = c;
        return c;
    }

    protected abstract ObjectMarker CreateObjectMarker(VoxelArrayEditor voxelArray);
    protected abstract DynamicEntityComponent CreateEntityComponent(VoxelArray voxelArray);
}