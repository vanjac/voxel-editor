using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateAxis : TransformAxis
{
    private float startTouchAngle;
    private float startAxisAngle;
    private bool moving;

    public override void Update()
    {
        base.Update();
        if (!moving)
        {
            float diff = 0 - GetAxisAngle(); // TODO
            while (diff > 180)
                diff -= 360;
            while (diff < -180)
                diff += 360;
            SetAxisAngle(GetAxisAngle() + diff / 4.0f);
        }
    }

    public override void TouchDown(Touch touch)
    {
        startTouchAngle = GetTouchAngle(touch.position);
        startAxisAngle = GetAxisAngle();
        moving = true;
    }

    public override void TouchUp()
    {
        moving = false;
    }

    public override void TouchDrag(Touch touch)
    {
        float newTouchAngle = GetTouchAngle(touch.position);
        float newAxisAngle = startAxisAngle + newTouchAngle - startTouchAngle;
        SetAxisAngle(newAxisAngle);
    }

    private float GetTouchAngle(Vector2 pos)
    {
        Vector3 originScreenPos = mainCamera.WorldToScreenPoint(transform.position);
        Vector2 originScreenPos2 = new Vector2(originScreenPos.x, originScreenPos.y);
        return Vector2.SignedAngle(pos - originScreenPos2, Vector2.right);
    }

    private float GetAxisAngle()
    {
        return transform.rotation.eulerAngles.y;
    }

    private void SetAxisAngle(float angle)
    {
        transform.rotation = Quaternion.Euler(new Vector3(0, angle, 0));
    }
}