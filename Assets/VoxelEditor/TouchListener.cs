using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TouchListener : MonoBehaviour
{
    public enum TouchOperation
    {
        NONE, SELECT, CAMERA, GUI, MOVE
    }

    public VoxelArrayEditor voxelArray;

    public TouchOperation currentTouchOperation = TouchOperation.NONE;
    public MoveAxis movingAxis;
    private Transform pivot;

    private Voxel lastHitVoxel;
    private int lastHitFaceI;

    void Start()
    {
        pivot = transform.parent;
    }

    void Update ()
    {
        if (Input.touchCount == 0)
        {
            if (currentTouchOperation == TouchOperation.SELECT)
                voxelArray.TouchUp();
            currentTouchOperation = TouchOperation.NONE;
        }
        else if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            RaycastHit hit;
            bool rayHitSomething = Physics.Raycast(GetComponent<Camera>().ScreenPointToRay(Input.GetTouch(0).position), out hit);
            Voxel hitVoxel = null;
            int hitFaceI = -1;
            MoveAxis hitMoveAxis = null;
            if (rayHitSomething)
            {
                GameObject hitObject = hit.transform.gameObject;
                if (hitObject.tag == "Voxel")
                {
                    hitVoxel = hitObject.GetComponent<Voxel>();
                    hitFaceI = Voxel.FaceIForDirection(hit.normal);

                    if (hitFaceI == -1)
                        hitVoxel = null;
                }
                else if (hitObject.tag == "MoveAxis")
                {
                    hitMoveAxis = hitObject.GetComponent<MoveAxis>();
                }
            }
            if (hitVoxel != null)
            {
                lastHitVoxel = hitVoxel;
                lastHitFaceI = hitFaceI;
            }

            if (touch.phase == TouchPhase.Began)
            {
                if (currentTouchOperation == TouchOperation.SELECT)
                    voxelArray.TouchUp();
                // this seems to improve the reliability of double-taps when things are running slowly.
                // I think it's because there's not always a long enough gap between taps
                // for the touch operation to be cleared.
                currentTouchOperation = TouchOperation.NONE;
            }

            if (currentTouchOperation == TouchOperation.NONE)
            {
                if (GUIPanel.PanelContainingPoint(touch.position) != null)
                    currentTouchOperation = TouchOperation.GUI;
                else if (touch.phase != TouchPhase.Moved && touch.phase != TouchPhase.Ended && touch.tapCount == 1)
                { } // wait until moved or released, in case a multitouch operation is about to begin
                else if (!rayHitSomething)
                {
                    voxelArray.TouchDown(null, -1);
                }
                else if (hitVoxel != null)
                {
                    if (touch.tapCount == 1)
                    {
                        currentTouchOperation = TouchOperation.SELECT;
                        voxelArray.TouchDown(hitVoxel, hitFaceI);
                    }
                    else if (touch.tapCount == 2)
                    {
                        voxelArray.DoubleTouch(hitVoxel, hitFaceI);
                    }
                }
                else if (hitMoveAxis != null)
                {
                    if (touch.tapCount == 1)
                    {
                        currentTouchOperation = TouchOperation.MOVE;
                        movingAxis = hitMoveAxis;
                        movingAxis.TouchDown(touch);
                    }
                    else if (touch.tapCount == 2)
                    {
                        voxelArray.DoubleTouch(lastHitVoxel, lastHitFaceI);
                    }
                }
            } // end if currentTouchOperation == NONE

            else if (currentTouchOperation == TouchOperation.SELECT)
            {
                if (hitVoxel != null)
                {
                    voxelArray.TouchDrag(hitVoxel, hitFaceI);
                }
            }
            else if (currentTouchOperation == TouchOperation.MOVE)
            {
                movingAxis.TouchDrag(touch);
            }
        } // end if touch count is 1
        else if (Input.touchCount == 2)
        {
            if (currentTouchOperation == TouchOperation.NONE)
                currentTouchOperation = TouchOperation.CAMERA;
            if (currentTouchOperation != TouchOperation.CAMERA)
                return;

            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

            float scaleFactor = Mathf.Pow(1.005f, deltaMagnitudeDiff);
            if (scaleFactor != 1)
                pivot.localScale *= scaleFactor;

            Vector3 move = (touchZero.deltaPosition + touchOne.deltaPosition) / 2;
            Vector3 pivotRotationEuler = pivot.rotation.eulerAngles;
            pivotRotationEuler.y += move.x * 0.3f;
            pivotRotationEuler.x -= move.y * 0.3f;
            if (pivotRotationEuler.x > 90 && pivotRotationEuler.x < 180)
                pivotRotationEuler.x = 90;
            if (pivotRotationEuler.x < -90 || (pivotRotationEuler.x > 180 && pivotRotationEuler.x < 270))
                pivotRotationEuler.x = -90;
            pivot.rotation = Quaternion.Euler(pivotRotationEuler);
        }
        else if (Input.touchCount == 3)
        {
            if (currentTouchOperation == TouchOperation.NONE)
                currentTouchOperation = TouchOperation.CAMERA;
            if (currentTouchOperation != TouchOperation.CAMERA)
                return;

            Vector2 move = new Vector2(0, 0);
            for (int i = 0; i < 3; i++)
                move += Input.GetTouch(i).deltaPosition;
            move /= 3;
            pivot.position -= move.x * pivot.right * pivot.localScale.z / 60;
            pivot.position -= move.y * pivot.up * pivot.localScale.z / 60;

            // as the camera is moving, adjust the depth of the pivot point to the depth at the center of the screen
            RaycastHit hit;
            Debug.DrawRay(transform.position, transform.forward * 20, Color.black);
            if (Physics.Raycast(transform.position, transform.forward, out hit))
            {
                float currentDistanceToCamera = (pivot.position - transform.position).magnitude;
                float newDistanceToCamera = (hit.point - transform.position).magnitude;
                pivot.position = hit.point;
                pivot.localScale *= newDistanceToCamera / currentDistanceToCamera;
            }
        }
    }
}
