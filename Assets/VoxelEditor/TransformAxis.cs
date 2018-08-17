using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class TransformAxis : MonoBehaviour
{
    public VoxelArrayEditor voxelArray;
    public Camera mainCamera;
    private LineRenderer lineRenderer;

    void Start()
    {
        lineRenderer = GetComponent<LineRenderer>();
        UpdateSize();
    }

    void OnEnable()
    {
        if (lineRenderer != null)
            UpdateSize();
    }

    public virtual void Update()
    {
        UpdateSize();
    }

    private void UpdateSize()
    {
        float distanceToCam = (transform.position - mainCamera.transform.position).magnitude;
        transform.localScale = Vector3.one * distanceToCam / 4;
        lineRenderer.startWidth = lineRenderer.endWidth = distanceToCam / 40;
    }

    public abstract void TouchDown(Touch touch);
    public abstract void TouchUp();
    public abstract void TouchDrag(Touch touch);
}