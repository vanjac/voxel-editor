using System.Collections.Generic;
using UnityEngine;

public class PropObject : ObjectEntity
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Prop", typeof(PropObject))
    {
        displayName = s => s.PropName,
        description = s => s.PropDesc,
        iconName = "table-chair",
    };
    public override PropertiesObjectType ObjectType => objectType;

    public string modelName = "";

    public PropObject()
    {
        paint.material = ResourcesDirectory.InstantiateMaterial(
            ResourcesDirectory.FindMaterial("MATTE", true));
        paint.material.color = Color.white;
    }

    public override IEnumerable<Property> Properties() =>
        Property.JoinProperties(base.Properties(), new Property[]
        {
            new Property("mdl", s => "Model",
                () => modelName,
                v =>
                {
                    modelName = (string)v;
                    if (marker)
                    {
                        var mesh = GetMesh();
                        marker.GetComponent<MeshFilter>().mesh = mesh;
                        marker.GetComponent<MeshCollider>().sharedMesh = mesh;
                    }
                },
                PropertyGUIs.Text),
        });
    
    private Mesh GetMesh() => Resources.Load<Mesh>("GameAssets/Models/" + modelName);
    
    private GameObject CreatePropObject()
    {
        var mesh = GetMesh();
        GameObject go = new GameObject(modelName);
        var meshFilter = go.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;
        go.AddComponent<MeshRenderer>();
        var collider = go.AddComponent<MeshCollider>();
        collider.sharedMesh = mesh;
        collider.convex = true;
        return go;
    }

    protected override ObjectMarker CreateObjectMarker(VoxelArrayEditor voxelArray) =>
        CreatePropObject().AddComponent<ObjectMarker>();

    protected override DynamicEntityComponent CreateEntityComponent(VoxelArray voxelArray) =>
        CreatePropObject().AddComponent<PropComponent>();
}

public class PropComponent : DynamicEntityComponent
{
    public override void Start()
    {
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<Collider>().isTrigger = true;
        // a rigidBody is required for collision detection
        Rigidbody rigidBody = gameObject.AddComponent<Rigidbody>();
        // no physics by default (could be disabled by a Physics behavior)
        rigidBody.isKinematic = true;
        base.Start();
    }
}
