using System.Collections.Generic;
using UnityEngine;

public class ObjectMarker : MonoBehaviour, VoxelArrayEditor.Selectable {
    public ObjectEntity objectEntity; // set when created
    private bool selected = false;

    public bool Equals(VoxelArrayEditor.Selectable other) => ReferenceEquals(this, other);

    public Bounds GetBounds() => new Bounds(objectEntity.position, Vector3.zero);

    public void Start() {
        UpdateMarker();
    }

    public void SelectionStateUpdated(VoxelArray voxelArray) {
        selected = voxelArray.IsSelected(this);
        UpdateMaterials();
    }

    public void UpdateMarker() {
        transform.position = objectEntity.PositionInEditor();
        transform.rotation = Quaternion.Euler(new Vector3(0, objectEntity.rotation, 0));
        UpdateMaterials();
    }

    private void UpdateMaterials() {
        Renderer renderer = GetComponentInChildren<Renderer>();
        if (renderer == null || objectEntity == null) {
            return;
        }
        List<Material> materials = new List<Material>();
        int layer = 0; // default
        if (objectEntity.highlight != Color.clear && !selected) {
            materials.Add(objectEntity.highlightMaterial);
        } else if (objectEntity != null && objectEntity.xRay) {
            materials.Add(VoxelComponent.xRayMaterial);
            layer = 8; // XRay layer
        } else {
            if (objectEntity.paint.material != null) {
                materials.Add(objectEntity.paint.material);
            } else {
                layer = 10; // TransparentObject
            }

            if (objectEntity.paint.overlay != null) {
                materials.Add(objectEntity.paint.overlay);
            }
        }
        if (selected) {
            materials.Add(VoxelComponent.selectedMaterial);
        }
        renderer.materials = materials.ToArray();

        gameObject.layer = layer;
        Collider collider = GetComponentInChildren<Collider>();
        if (collider != null) {
            collider.gameObject.layer = layer;
        }
    }
}