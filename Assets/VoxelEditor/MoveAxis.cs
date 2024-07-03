using UnityEngine;

public class MoveAxis : TransformAxis {
    public Vector3 forwardDirection;
    public float moveCount = 0;
    private bool moving;

    public override void Update() {
        base.Update();
        if (!moving) {
            float move = (GetOriginPosition() - GetAxisPosition()) / 4.0f;
            transform.position += forwardDirection * move;
        }
    }

    private float GetAxisPosition() => Vector3.Dot(transform.position, forwardDirection);

    private float GetOriginPosition() => Vector3.Dot(transform.parent.position, forwardDirection);

    // below Touch functions are called by Touch Listener

    public override void TouchDown(Touch touch) {
        moveCount = 0;
        moving = true;
    }

    public override void TouchUp() {
        moving = false;
    }

    public override void TouchDrag(Touch touch) {
        float distanceToCam = (transform.position - mainCamera.transform.position).magnitude;

        Vector3 originScreenPos = mainCamera.WorldToScreenPoint(transform.position);
        Vector3 offsetScreenPos = mainCamera.WorldToScreenPoint(transform.position + forwardDirection);
        Vector3 screenMoveVector = offsetScreenPos - originScreenPos;

        float moveAmount = Vector3.Dot(touch.deltaPosition * 1.5f / mainCamera.pixelHeight, screenMoveVector.normalized);
        transform.position += forwardDirection * moveAmount * distanceToCam;

        float adjustScale = voxelArray.AllowedAdjustScale();

        float currentPosition = GetAxisPosition();
        float prevPosition = GetOriginPosition();
        int count = 0;
        if (currentPosition - prevPosition > adjustScale) {
            count = (int)Mathf.Floor((currentPosition - prevPosition) / adjustScale);
            voxelArray.Adjust(Vector3Int.RoundToInt(forwardDirection), count, adjustScale);
        } else if (currentPosition - prevPosition < -adjustScale) {
            count = -(int)Mathf.Floor((prevPosition - currentPosition) / adjustScale);
            voxelArray.Adjust(Vector3Int.RoundToInt(-forwardDirection), -count, adjustScale);
        }
        if (count != 0) {
            float scaledCount = count * adjustScale;
            transform.parent.position += forwardDirection * scaledCount;
            transform.position -= forwardDirection * scaledCount;
            moveCount += scaledCount;
        }
    }
}
