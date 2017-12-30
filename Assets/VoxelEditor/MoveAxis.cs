using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAxis : MonoBehaviour
{
    public Vector3 forwardDirection;
    public Camera mainCamera;
    public VoxelArrayEditor voxelArray;
    private float lastUpdatePosition;
    private LineRenderer lineRenderer;
    public int moveCount = 0;

    void Start ()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        float distanceToCam = (transform.position - mainCamera.transform.position).magnitude;
        transform.localScale = Vector3.one * distanceToCam / 5;
        lineRenderer.startWidth = lineRenderer.endWidth = distanceToCam / 40;
    }

    float GetPosition()
    {
        return Vector3.Dot(transform.parent.position, forwardDirection);
    }

    // below Touch functions are called by Touch Listener

    public void TouchDown(Touch touch)
    {
        lastUpdatePosition = GetPosition();
        moveCount = 0;
    }

    public void TouchDrag(Touch touch)
    {
        float distanceToCam = (transform.position - mainCamera.transform.position).magnitude;

        Vector3 originScreenPos = mainCamera.WorldToScreenPoint(transform.position);
        Vector3 offsetScreenPos = mainCamera.WorldToScreenPoint(transform.position + forwardDirection);
        Vector3 screenMoveVector = offsetScreenPos - originScreenPos;

        float moveAmount = Vector3.Dot(touch.deltaPosition, screenMoveVector.normalized);
        transform.parent.position += forwardDirection * moveAmount * distanceToCam / 500;

        float position = GetPosition();
        while (position - lastUpdatePosition > 1)
        {
            lastUpdatePosition++;
            moveCount++;
            voxelArray.Adjust(forwardDirection);
        }
        while(position - lastUpdatePosition < -1)
        {
            lastUpdatePosition--;
            moveCount--;
            voxelArray.Adjust(-forwardDirection);
        }
    }
}
