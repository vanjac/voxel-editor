using System.Collections.Generic;
using UnityEngine;

// TODO: combine with Pivot?
public enum ObjectAlignment
{
    Bottom, Center, Top, Default
}

public class PropObject : ObjectEntity
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Prop", typeof(PropObject))
    {
        displayName = s => s.PropName,
        description = s => s.PropDesc,
        iconName = "traffic-cone",
    };
    public override PropertiesObjectType ObjectType => objectType;

    public string modelName = "SM_Primitive_SoccerBall_01";
    public ObjectAlignment align = ObjectAlignment.Default;

    public PropObject()
    {
        paint.material = ResourcesDirectory.InstantiateMaterial(
            ResourcesDirectory.FindMaterial("GLOSSY", true));
        paint.material.color = Color.white;
    }

    public override IEnumerable<Property> Properties() =>
        Property.JoinProperties(base.Properties(), new Property[]
        {
            new Property("mdl", s => s.PropModel,
                () => modelName,
                v =>
                {
                    modelName = (string)v;
                    UpdateMarkerMesh();
                    UpdateMarkerPosition();
                },
                PropertyGUIs.Model),
            new Property("ali", s => s.PropPivot,
                () => align,
                v =>
                {
                    align = (ObjectAlignment)v;
                    UpdateMarkerPosition();
                },
                PropertyGUIs.EnumIcons(new Texture[]{
                    GUIPanel.IconSet.alignBottom,
                    GUIPanel.IconSet.alignCenter,
                    GUIPanel.IconSet.alignTop,
                    GUIPanel.IconSet.origin,
                })),
        });
    
    private Mesh GetMesh() => Resources.Load<Mesh>("GameAssets/Models/" + modelName)
        ?? Resources.Load<Mesh>("GameAssets/error_model");
    
    private GameObject CreatePropObject()
    {
        var mesh = GetMesh();
        GameObject rootGO = new GameObject(modelName);
        GameObject meshGO = new GameObject("mesh");
        meshGO.tag = "ObjectMarker";
        meshGO.transform.SetParent(rootGO.transform, false);
        meshGO.transform.localPosition = GetMeshPositionOffset(mesh);

        var meshFilter = meshGO.AddComponent<MeshFilter>();
        meshFilter.mesh = mesh;
        meshGO.AddComponent<MeshRenderer>();
        var collider = meshGO.AddComponent<MeshCollider>();
        collider.sharedMesh = mesh;
        collider.convex = true;
        return rootGO;
    }

    private Vector3 GetMeshPositionOffset(Mesh mesh) {
        var yPos = align switch
        {
            ObjectAlignment.Bottom => -mesh.bounds.min.y,
            ObjectAlignment.Center => -mesh.bounds.center.y,
            ObjectAlignment.Top    => -mesh.bounds.max.y,
            _ => 0,
        };
        return new Vector3(0, yPos, 0);
    }

    private void UpdateMarkerMesh()
    {
        if (marker)
        {
            var mesh = GetMesh();
            marker.GetComponentInChildren<MeshFilter>().mesh = mesh;
            marker.GetComponentInChildren<MeshCollider>().sharedMesh = mesh;
        }
    }

    private void UpdateMarkerPosition()
    {
        if (marker)
        {
            var meshFilter = marker.GetComponentInChildren<MeshFilter>();
            meshFilter.transform.localPosition = GetMeshPositionOffset(meshFilter.mesh);
        }
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
        GetComponentInChildren<MeshRenderer>().enabled = false;
        GetComponentInChildren<Collider>().isTrigger = true;
        // a rigidBody is required for collision detection
        Rigidbody rigidBody = gameObject.AddComponent<Rigidbody>();
        // no physics by default (could be disabled by a Physics behavior)
        rigidBody.isKinematic = true;
        base.Start();
    }
}
