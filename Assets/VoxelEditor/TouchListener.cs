using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TouchListener : MonoBehaviour
{
    private const float MAX_ZOOM = 20.0f;
    private const float MIN_ZOOM = .05f;

    public enum TouchOperation
    {
        NONE, SELECT, CAMERA, GUI, MOVE
    }

    public VoxelArrayEditor voxelArray;

    public TouchOperation currentTouchOperation = TouchOperation.NONE;
    public MoveAxis movingAxis;
    private Transform pivot;
    private Camera camera;

    private Voxel lastHitVoxel;
    private int lastHitFaceI;

    void Start()
    {
        pivot = transform.parent;
        camera = GetComponent<Camera>();
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
            bool rayHitSomething = Physics.Raycast(camera.ScreenPointToRay(Input.GetTouch(0).position), out hit);
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
                        UpdateZoomDepth();
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
            {
                currentTouchOperation = TouchOperation.CAMERA;
                UpdateZoomDepth();
            }
            if (currentTouchOperation != TouchOperation.CAMERA)
                return;

            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

            float scaleFactor = Mathf.Pow(1.005f, deltaMagnitudeDiff / camera.pixelHeight * 700f);
            if (scaleFactor != 1)
            {
                pivot.localScale *= scaleFactor;
                if (pivot.localScale.x > MAX_ZOOM)
                    pivot.localScale = new Vector3(MAX_ZOOM, MAX_ZOOM, MAX_ZOOM);
                if (pivot.localScale.x < MIN_ZOOM)
                    pivot.localScale = new Vector3(MIN_ZOOM, MIN_ZOOM, MIN_ZOOM);
            }

            Vector3 move = (touchZero.deltaPosition + touchOne.deltaPosition) / 2;
            move *= 300f;
            move /= camera.pixelHeight;
            Vector3 pivotRotationEuler = pivot.rotation.eulerAngles;
            pivotRotationEuler.y += move.x;
            pivotRotationEuler.x -= move.y;
            if (pivotRotationEuler.x > 90 && pivotRotationEuler.x < 180)
                pivotRotationEuler.x = 90;
            if (pivotRotationEuler.x < -90 || (pivotRotationEuler.x > 180 && pivotRotationEuler.x < 270))
                pivotRotationEuler.x = -90;
            pivot.rotation = Quaternion.Euler(pivotRotationEuler);
        }
        else if (Input.touchCount == 3)
        {
            if (currentTouchOperation == TouchOperation.NONE)
            {
                currentTouchOperation = TouchOperation.CAMERA;
                UpdateZoomDepth();
            }
            if (currentTouchOperation != TouchOperation.CAMERA)
                return;

            Vector2 move = new Vector2(0, 0);
            for (int i = 0; i < 3; i++)
                move += Input.GetTouch(i).deltaPosition;
            move *= 4.0f;
            move /= camera.pixelHeight;
            pivot.position -= move.x * pivot.right * pivot.localScale.z;
            pivot.position -= move.y * pivot.up * pivot.localScale.z;
        }
    }

    private void UpdateZoomDepth()
    {
        // adjust the depth of the pivot point to the depth at the average point between the fingers
        int touchCount = Input.touchCount;
        Vector2 avg = Vector2.zero;
        for (int i = 0; i < touchCount; i++)
            avg += Input.GetTouch(i).position;
        if (touchCount > 0)
            avg /= touchCount;
        else
            avg = new Vector2(camera.pixelWidth / 2, camera.pixelHeight / 2);

        Ray ray = camera.ScreenPointToRay(avg);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit))
        {
            float currentDistanceToCamera = (pivot.position - transform.position).magnitude;
            float newDistanceToCamera = (hit.point - transform.position).magnitude;
            pivot.position = (pivot.position - transform.position).normalized * newDistanceToCamera + transform.position;
            pivot.localScale *= newDistanceToCamera / currentDistanceToCamera;
        }
    }
}
