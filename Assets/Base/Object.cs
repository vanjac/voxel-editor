using System.Collections;
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

    public override void UpdateEntity()
    {
        if (marker != null)
        {
            marker.transform.position = position;
            marker.UpdateMaterials();
        }
    }

    public abstract void InitObjectMarker();
}