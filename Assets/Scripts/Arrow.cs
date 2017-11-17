using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Arrow : MonoBehaviour {

    public Vector3 forwardDirection;
    public Camera camera;
    public VoxelArray voxelArray;
    float lastUpdatePosition;
    Vector3 prevMousePosition;
    LineRenderer lineRenderer;

	// Use this for initialization
	void Start () {
        lineRenderer = GetComponent<LineRenderer>();
	}

    void Update()
    {
        float distanceToCam = (transform.position - camera.transform.position).magnitude;
        transform.localScale = Vector3.one * distanceToCam / 5;
        lineRenderer.startWidth = lineRenderer.endWidth = distanceToCam / 40;
    }

    float GetPosition()
    {
        return Vector3.Dot(transform.parent.position, forwardDirection);
    }

    void OnMouseDown()
    {
        if (Input.touchCount > 1)
            return;
        lastUpdatePosition = GetPosition();
        prevMousePosition = Input.mousePosition;
    }

    void OnMouseDrag()
    {
        if (Input.touchCount > 1)
            return;
        float distanceToCam = (transform.position - camera.transform.position).magnitude;

        Vector3 mouseMove = Input.mousePosition - prevMousePosition;
        prevMousePosition = Input.mousePosition;

        Vector3 originScreenPos = camera.WorldToScreenPoint(transform.position);
        Vector3 offsetScreenPos = camera.WorldToScreenPoint(transform.position + forwardDirection);
        Vector3 screenMoveVector = offsetScreenPos - originScreenPos;

        float moveAmount = Vector3.Dot(mouseMove, screenMoveVector.normalized);
        transform.parent.position += forwardDirection * moveAmount * distanceToCam / 500;

        float position = GetPosition();
        while (position - lastUpdatePosition > 1)
        {
            lastUpdatePosition++;
            voxelArray.Adjust(forwardDirection);
        }
        while(position - lastUpdatePosition < -1)
        {
            lastUpdatePosition--;
            voxelArray.Adjust(-forwardDirection);
        }
    }
}
