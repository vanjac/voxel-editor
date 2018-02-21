using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectMarker : MonoBehaviour, VoxelArrayEditor.Selectable
{
    public bool addSelected { get; set; }
    public bool storedSelected { get; set; }
    public bool selected
    {
        get
        {
            return addSelected || storedSelected;
        }
    }

    public void SelectionStateUpdated()
    {
        // TODO
    }

    public Bounds GetBounds()
    {
        // TODO
        return new Bounds(transform.position, Vector3.zero);
    }
}