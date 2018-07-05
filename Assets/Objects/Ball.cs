using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallObject : ObjectEntity
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Ball", "A sphere with a custom material", "circle-outline", typeof(BallObject));

    // make sure CreatePrimitive works: https://docs.unity3d.com/ScriptReference/GameObject.CreatePrimitive.html
    private MeshFilter fixEverything1;
    private MeshRenderer fixEverything2;
    private SphereCollider fixEverything3;

    public override PropertiesObjectType ObjectType()
    {
        return objectType;
    }

    private GameObject ObjectTemplate(VoxelArray voxelArray)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        obj.GetComponent<MeshRenderer>().materials = new Material[]
        {
            ResourcesDirectory.MakeCustomMaterial(ColorMode.MATTE)
        };
        return obj;
    }

    protected override ObjectMarker CreateObjectMarker(VoxelArrayEditor voxelArray)
    {
        GameObject markerObject = ObjectTemplate(voxelArray);
        return markerObject.AddComponent<ObjectMarker>();
    }

    protected override DynamicEntityComponent CreateEntityComponent(VoxelArray voxelArray)
    {
        GameObject obj = ObjectTemplate(voxelArray);
        return obj.AddComponent<BallComponent>();
    }
}

public class BallComponent : DynamicEntityComponent
{

}