using System.Collections.Generic;
using UnityEngine;

public class ObjectMarker : MonoBehaviour, VoxelArrayEditor.Selectable
{
    public ObjectEntity objectEntity; // set when created
    private bool selected = false;

    public bool Equals(VoxelArrayEditor.Selectable other) => ReferenceEquals(this, other);

    public Bounds GetBounds() => new Bounds(objectEntity.position, Vector3.zero);

    public void Start()
    {
        UpdateMarker();
    }

    public void SelectionStateUpdated(VoxelArray voxelArray)
    {
        selected = voxelArray.IsSelected(this);
        UpdateMaterials();
    }

    public void UpdateMarker()
    {
        transform.position = objectEntity.PositionInEditor();
        transform.rotation = Quaternion.Euler(new Vector3(0, objectEntity.rotation, 0));
        UpdateMaterials();
    }

    private void UpdateMaterials()
    {
        Renderer renderer = GetComponentInChildren<Renderer>();
        if (renderer == null || objectEntity == null)
            return;
        List<Material> materials = new List<Material>();
        if (objectEntity.highlight != Color.clear && !selected)
        {
            materials.Add(objectEntity.highlightMaterial);
            gameObject.layer = 0; // default
        }
        else if (objectEntity != null && objectEntity.xRay)
        {
            materials.Add(VoxelComponent.xRayMaterial);
            gameObject.layer = 8; // XRay layer
        }
        else
        {
            if (objectEntity.paint.material != null)
            {
                materials.Add(objectEntity.paint.material);
                gameObject.layer = 0; // default
            }
            else
                gameObject.layer = 10; // TransparentObject
            if (objectEntity.paint.overlay != null)
                materials.Add(objectEntity.paint.overlay);
        }
        if (selected)
            materials.Add(VoxelComponent.selectedMaterial);
        renderer.materials = materials.ToArray();
    }
}