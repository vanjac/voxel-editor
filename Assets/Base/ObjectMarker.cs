using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectMarker : MonoBehaviour, VoxelArrayEditor.Selectable
{
    public Entity entity = new ObjectEntity();

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
            // TODO
            return new Bounds(transform.position, Vector3.zero);
        }
    }

    private Material[] storedMaterials;

    public void Start()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            storedMaterials = (Material[])renderer.materials.Clone();
        }
    }

    public void SelectionStateUpdated()
    {
        Renderer renderer = GetComponent<Renderer>();
        if (renderer != null)
        {
            Debug.Log(selected);
            if (selected)
            {
                Material[] newMaterials = new Material[renderer.materials.Length];
                for (int i = 0; i < newMaterials.Length; i++)
                    newMaterials[i] = Voxel.selectedMaterial;
                renderer.materials = newMaterials;
            }
            else
                renderer.materials = storedMaterials;
        }
    }
}