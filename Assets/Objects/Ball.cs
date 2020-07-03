using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallObject : ObjectEntity
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Ball", "A sphere with a custom material", "circle-outline", typeof(BallObject));

    private Material material;

    // make sure CreatePrimitive works: https://docs.unity3d.com/ScriptReference/GameObject.CreatePrimitive.html
    private MeshFilter fixEverything1;
    private MeshRenderer fixEverything2;
    private SphereCollider fixEverything3;

    public BallObject()
    {
        material = ResourcesDirectory.MakeCustomMaterial(ColorMode.MATTE, true);
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
                PropertyGUIs.Material("Materials", true,
                    MaterialSelectorGUI.ColorModeSet.OBJECT, true))
        });
    }

    private GameObject ObjectTemplate(VoxelArray voxelArray)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        obj.name = "Ball";
        obj.GetComponent<MeshRenderer>().materials = new Material[] { material };
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
