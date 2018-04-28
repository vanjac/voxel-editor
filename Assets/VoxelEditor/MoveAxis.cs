using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAxis : MonoBehaviour
{
    public Vector3 forwardDirection;
    public Camera mainCamera;
    public VoxelArrayEditor voxelArray;
    private LineRenderer lineRenderer;
    public int moveCount = 0;
    private bool moving;

    void Start ()
    {
        lineRenderer = GetComponent<LineRenderer>();
    }

    void Update()
    {
        float distanceToCam = (transform.position - mainCamera.transform.position).magnitude;
        transform.localScale = Vector3.one * distanceToCam / 4;
        lineRenderer.startWidth = lineRenderer.endWidth = distanceToCam / 40;

        if (!moving)
        {
            float move = (GetOriginPosition() - GetAxisPosition()) / 4.0f;
            transform.position += forwardDirection * move;
        }
    }

    float GetAxisPosition()
    {
        return Vector3.Dot(transform.position, forwardDirection);
    }

    float GetOriginPosition()
    {
        return Vector3.Dot(transform.parent.position, forwardDirection);
    }

    // below Touch functions are called by Touch Listener

    public void TouchDown(Touch touch)
    {
        moveCount = 0;
        moving = true;
    }

    public void TouchUp()
    {
        moving = false;
    }

    public void TouchDrag(Touch touch)
    {
        float distanceToCam = (transform.position - mainCamera.transform.position).magnitude;

        Vector3 originScreenPos = mainCamera.WorldToScreenPoint(transform.position);
        Vector3 offsetScreenPos = mainCamera.WorldToScreenPoint(transform.position + forwardDirection);
        Vector3 screenMoveVector = offsetScreenPos - originScreenPos;

        float moveAmount = Vector3.Dot(touch.deltaPosition * 1.5f / mainCamera.pixelHeight, screenMoveVector.normalized);
        transform.position += forwardDirection * moveAmount * distanceToCam;

        float currentPosition = GetAxisPosition();
        float prevPosition = GetOriginPosition();
        while (currentPosition - prevPosition > 1)
        {
            transform.parent.position += forwardDirection;
            transform.position -= forwardDirection;
            moveCount++;
            currentPosition -= 1;
            voxelArray.Adjust(forwardDirection);
        }
        while (currentPosition - prevPosition < -1)
        {
            transform.parent.position -= forwardDirection;
            transform.position += forwardDirection;
            moveCount--;
            currentPosition += 1;
            voxelArray.Adjust(-forwardDirection);
        }
    }
}
