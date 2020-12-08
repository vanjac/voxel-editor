using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallObject : ObjectEntity
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Ball", "A sphere with a custom material", "circle-outline", typeof(BallObject));

    private Material material;


    public BallObject()
    {
        material = ResourcesDirectory.InstantiateMaterial(
            ResourcesDirectory.FindMaterial("MATTE_overlay", true));
        material.color = Color.red;
    }

    public override PropertiesObjectType ObjectType()
    {
        return objectType;
    }

    public override ICollection<Property> Properties()
    {
        return Property.JoinProperties(base.Properties(), new Property[]
        {
            new Property("mat", "Material",
                () => material,
                v =>
                {
                    material = (Material)v;
                    if (marker != null)
                    {
                        marker.storedMaterials[0] = material;
                        marker.UpdateMarker();
                    }
                },
                PropertyGUIs.Material("Overlays", true))
        });
    }

    private GameObject ObjectTemplate(VoxelArray voxelArray)
    {
        return GameObject.Instantiate(Resources.Load<GameObject>("ObjectPrefabs/Ball"));
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
    public override void Start()
    {
        GetComponent<MeshRenderer>().enabled = false;
        GetComponent<SphereCollider>().isTrigger = true;
        // a rigidBody is required for collision detection
        Rigidbody rigidBody = gameObject.AddComponent<Rigidbody>();
        // no physics by default (could be disabled by a Physics behavior)
        rigidBody.isKinematic = true;
        base.Start();
    }
}
