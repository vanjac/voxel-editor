﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectMarker : MonoBehaviour, VoxelArrayEditor.Selectable
{
    public ObjectEntity objectEntity; // set when created

    public bool addSelected
    {
        get
        {
            return objectEntity.paint.addSelected;
        }
        set
        {
            objectEntity.paint.addSelected = value;
        }
    }
    public bool storedSelected
    {
        get
        {
            return objectEntity.paint.storedSelected;
        }
        set
        {
            objectEntity.paint.storedSelected = value;
        }
    }
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

    public void Start()
    {
        UpdateMarker();
    }

    public void SelectionStateUpdated()
    {
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
        Renderer renderer = GetComponent<Renderer>();
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