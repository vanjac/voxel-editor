using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallObject : ObjectEntity
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Ball", "A sphere which can be painted", "circle-outline", typeof(BallObject));

    public BallObject()
    {
        paint.baseLayer.material = ResourcesDirectory.FindMaterial("MATTE", true);
        paint.baseLayer.color = Color.red;
    }

    public override PropertiesObjectType ObjectType()
    {
        return objectType;
    }

    public override ICollection<Property> DeprecatedProperties()
    {
        return Property.JoinProperties(base.DeprecatedProperties(), new Property[]
        {
            new Property("mat", "Material",
                () => paint.baseLayer.material == null ? paint.overlay.material : paint.baseLayer.material,
                v =>
                {
                    var mat = (Material)v;
                    paint.Clear();
                    if (mat.renderQueue >= (int)UnityEngine.Rendering.RenderQueue.AlphaTest)
                        paint.overlay.material = mat;
                    else
                        paint.baseLayer.material = mat;
                    if (marker != null)
                        marker.UpdateMarker();
                },
                PropertyGUIs.Empty)
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
