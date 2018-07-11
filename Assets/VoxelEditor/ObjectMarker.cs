using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectMarker : MonoBehaviour, VoxelArrayEditor.Selectable
{
    public ObjectEntity objectEntity; // set when created

    public bool addSelected { get; set; }
    public bool storedSelected { get; set; }
    public bool selected
    {
        get
        {
            return addSelected || storedSelected;
        }
    }

    public Bounds bounds
    {
        get
        {
            return new Bounds(objectEntity.position + new Vector3(0.5f, 0.5f, 0.5f),
                Vector3.zero);
        }
    }

    public Material[] storedMaterials;

    public void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            storedMaterials = (Material[])renderer.materials.Clone();
        }
        UpdateMarker();
    }

    public void SelectionStateUpdated()
    {
        UpdateMaterials();
    }

    public void UpdateMarker()
    {
        transform.position = objectEntity.position + new Vector3(0.5f, 0.5f, 0.5f)
            + objectEntity.PositionOffset();
        UpdateMaterials();
    }

    private void UpdateMaterials()
    {
        if (storedMaterials == null)
            // storedMaterials hasn't been initialized yet, so we wouldn't be able to return to normal state
            return;
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            if (selected)
            {
                SetAllMaterials(renderer, Voxel.selectedMaterial);
                gameObject.layer = 8; // XRay layer, because selected material makes it transparent
            }
            else if (objectEntity != null && objectEntity.xRay)
            {
                SetAllMaterials(renderer, Voxel.xRayMaterial);
                gameObject.layer = 8; // XRay layer
            }
            else
            {
                renderer.materials = storedMaterials;
                gameObject.layer = 0; // default
            }
        }
    }

    private void SetAllMaterials(Renderer renderer, Material mat)
    {
        Material[] newMaterials = new Material[renderer.materials.Length];
        for (int i = 0; i < newMaterials.Length; i++)
            newMaterials[i] = mat;
        renderer.materials = newMaterials;
    }
}