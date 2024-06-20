using System.Collections.Generic;
using UnityEngine;

public class BallObject : ObjectEntity
{
    public static new PropertiesObjectType objectType = new PropertiesObjectType(
        "Ball", typeof(BallObject))
    {
        displayName = s => s.BallName,
        description = s => s.BallDesc,
        longDescription = s => s.BallLongDesc,
        iconName = "circle-outline",
    };
    public override PropertiesObjectType ObjectType => objectType;

    public BallObject()
    {
        paint.material = ResourcesDirectory.InstantiateMaterial(
            ResourcesDirectory.FindMaterial("MATTE", true));
        paint.material.color = Color.red;
    }

    public override Bounds PlacementBounds() => new Bounds(Vector3.zero, Vector3.one);

    public override IEnumerable<Property> DeprecatedProperties() =>
        Property.JoinProperties(base.DeprecatedProperties(), new Property[]
        {
            new Property("mat", GUIStringSet.Empty,
                () => paint.material == null ? paint.overlay : paint.material,
                v =>
                {
                    var mat = (Material)v;
                    if (mat.renderQueue >= (int)UnityEngine.Rendering.RenderQueue.AlphaTest)
                    {
                        paint.overlay = mat;
                        paint.material = null;
                    }
                    else
                    {
                        paint.material = mat;
                        paint.overlay = null;
                    }
                    if (marker != null)
                        marker.UpdateMarker();
                },
                PropertyGUIs.Material("Overlays", true))
        });

    private GameObject ObjectTemplate(VoxelArray voxelArray) =>
        GameObject.Instantiate(Resources.Load<GameObject>("ObjectPrefabs/Ball"));

    protected override ObjectMarker CreateObjectMarker(VoxelArrayEditor voxelArray)
    {
        GameObject markerObject = ObjectTemplate(voxelArray);
        return markerObject.AddComponent<ObjectMarker>();
    }

    protected override DynamicEntityComponent CreateEntityComponent(VoxelArray voxelArray)
    {
        GameObject obj = ObjectTemplate(voxelArray);
        return obj.AddComponent<PropComponent>();
    }
}
