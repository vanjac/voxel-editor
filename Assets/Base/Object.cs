using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectEntity : DynamicEntity
{
    public ObjectMarker marker; // set by ObjectMarker

    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Object", "An object not made of blocks", "circle", typeof(ObjectEntity));

    public override PropertiesObjectType ObjectType()
    {
        return objectType;
    }

    public override void UpdateEntity()
    {
        if (marker != null)
            marker.UpdateMaterials();
    }
}