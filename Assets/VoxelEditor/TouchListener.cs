using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TouchListener : MonoBehaviour
{
    private const float MAX_ZOOM = 20.0f;
    private const float MIN_ZOOM = .05f;
    private const int NO_XRAY_MASK = Physics.DefaultRaycastLayers & ~(1 << 8); // everything but XRay layer

    public enum TouchOperation
    {
        NONE, SELECT, CAMERA, GUI, MOVE
    }

    public VoxelArrayEditor voxelArray;

    public TouchOperation currentTouchOperation = TouchOperation.NONE;
    public MoveAxis movingAxis;
    public Transform pivot;
    private Camera cam;

    private Voxel lastHitVoxel;
    private int lastHitFaceI;
    private bool selectingXRay = true;

    void Start()
    {
        pivot = transform.parent;
        cam = GetComponent<Camera>();
    }

    void Update ()
    {
        if (Input.touchCount == 0)
        {
            if (currentTouchOperation == TouchOperation.SELECT)
                voxelArray.TouchUp();
            if (currentTouchOperation == TouchOperation.MOVE)
                movingAxis.TouchUp();
            currentTouchOperation = TouchOperation.NONE;
        }
        else if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            RaycastHit hit;
            if (currentTouchOperation != TouchOperation.SELECT)
                selectingXRay = true;
            bool rayHitSomething = Physics.Raycast(cam.ScreenPointToRay(Input.GetTouch(0).position),
                out hit, Mathf.Infinity, selectingXRay ? Physics.DefaultRaycastLayers : NO_XRAY_MASK);
            Voxel hitVoxel = null;
            int hitFaceI = -1;
            MoveAxis hitMoveAxis = null;
            ObjectMarker hitMarker = null;
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
                else if (hitObject.tag == "ObjectMarker")
                {
                    hitMarker = hitObject.GetComponent<ObjectMarker>();
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
                    voxelArray.TouchDown(null);
                }
                else if (hitVoxel != null)
                {
                    if (touch.tapCount == 1)
                    {
                        currentTouchOperation = TouchOperation.SELECT;
                        voxelArray.TouchDown(hitVoxel, hitFaceI);
                        selectingXRay = hitVoxel.substance != null && hitVoxel.substance.xRay;
                    }
                    else if (touch.tapCount == 2)
                    {
                        voxelArray.DoubleTouch(hitVoxel, hitFaceI);
                    }
                    else if (touch.tapCount == 3)
                    {
                        voxelArray.TripleTouch(hitVoxel, hitFaceI);
                    }
                    UpdateZoomDepth();
                }
                else if (hitMarker != null)
                {
                    currentTouchOperation = TouchOperation.SELECT;
                    voxelArray.TouchDown(hitMarker);
                    UpdateZoomDepth();
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
                    else if (touch.tapCount == 3)
                    {
                        voxelArray.TripleTouch(lastHitVoxel, lastHitFaceI);
                    }
                    UpdateZoomDepth();
                }
            } // end if currentTouchOperation == NONE

            else if (currentTouchOperation == TouchOperation.SELECT)
            {
                if (hitVoxel != null)
                {
                    voxelArray.TouchDrag(hitVoxel, hitFaceI);
                }
                if (hitMarker != null)
                {
                    voxelArray.TouchDrag(hitMarker);
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

            float scaleFactor = Mathf.Pow(1.005f, deltaMagnitudeDiff / cam.pixelHeight * 700f);
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
            move /= cam.pixelHeight;
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
            move /= cam.pixelHeight;
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
            avg = new Vector2(cam.pixelWidth / 2, cam.pixelHeight / 2);

        Ray ray = cam.ScreenPointToRay(avg);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, NO_XRAY_MASK))
        {
            float currentDistanceToCamera = (pivot.position - transform.position).magnitude;
            float newDistanceToCamera = (hit.point - transform.position).magnitude;
            pivot.position = (pivot.position - transform.position).normalized * newDistanceToCamera + transform.position;
            pivot.localScale *= newDistanceToCamera / currentDistanceToCamera;
        }
    }
}
