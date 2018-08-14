using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAxis : TransformAxis
{
    public Vector3 forwardDirection;
    public int moveCount = 0;
    private bool moving;

    public override void Update()
    {
        base.Update();
        if (!moving)
        {
            float move = (GetOriginPosition() - GetAxisPosition()) / 4.0f;
            transform.position += forwardDirection * move;
        }
    }

    private float GetAxisPosition()
    {
        return Vector3.Dot(transform.position, forwardDirection);
    }

    private float GetOriginPosition()
    {
        return Vector3.Dot(transform.parent.position, forwardDirection);
    }

    // below Touch functions are called by Touch Listener

    public override void TouchDown(Touch touch)
    {
        moveCount = 0;
        moving = true;
    }

    public override void TouchUp()
    {
        moving = false;
    }

    public override void TouchDrag(Touch touch)
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
