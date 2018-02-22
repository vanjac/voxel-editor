using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectEntity : DynamicEntity
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Object", "An object not made of blocks", "circle", typeof(ObjectEntity));

    public override PropertiesObjectType ObjectType()
    {
        return objectType;
    }
}