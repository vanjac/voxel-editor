using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TouchListener : MonoBehaviour {

    enum TouchOperation
    {
        NONE, SELECT, CAMERA, GUI, MOVE
    }

    public VoxelArray voxelArray;

    TouchOperation currentTouchOperation = TouchOperation.NONE;
    Arrow movingArrow;
    Transform pivot;

    void Start()
    {
        pivot = transform.parent;
    }

	void Update ()
    {
        if (Input.touchCount == 0)
        {
            currentTouchOperation = TouchOperation.NONE;
        }
        else if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            RaycastHit hit;
            bool rayHitSomething = Physics.Raycast(Camera.main.ScreenPointToRay(Input.GetTouch(0).position), out hit);
            Voxel hitVoxel = null;
            int hitFaceI = -1;
            Arrow hitArrow = null;
            if (rayHitSomething)
            {
                GameObject hitObject = hit.transform.gameObject;
                if (hitObject.tag == "Voxel")
                {
                    hitVoxel = hitObject.GetComponent<Voxel>();
                    hitFaceI = Voxel.FaceIForNormal(hit.normal);
                    if (hitFaceI == -1)
                        hitVoxel = null;
                }
                else if (hitObject.tag == "MoveAxis")
                {
                    hitArrow = hitObject.GetComponent<Arrow>();
                }
            }

            if (currentTouchOperation == TouchOperation.NONE)
            {
                if (GUIPanel.PanelContainingPoint(touch.position) != null)
                    currentTouchOperation = TouchOperation.GUI;
                else if (touch.phase != TouchPhase.Moved && touch.phase != TouchPhase.Ended)
                    ; // wait until moved or released, in case a multitouch operation is about to begin
                else if (!rayHitSomething)
                {
                    voxelArray.SelectBackground();
                }
                else if (hitVoxel != null)
                {
                    currentTouchOperation = TouchOperation.SELECT;
                    voxelArray.SelectDown(hitVoxel, hitFaceI);
                }
                else if (hitArrow != null)
                {
                    currentTouchOperation = TouchOperation.MOVE;
                    movingArrow = hitArrow;
                    movingArrow.TouchDown(touch);
                }
            }

            if (currentTouchOperation == TouchOperation.GUI)
            {
                GUIPanel panel = GUIPanel.PanelContainingPoint(touch.position);
                if (panel != null)
                    panel.scroll.y += touch.deltaPosition.y / panel.scaleFactor;
            }
            else if (currentTouchOperation == TouchOperation.SELECT)
            {
                if (hitVoxel != null)
                {
                    voxelArray.SelectDrag(hitVoxel, hitFaceI);
                }
            }
            else if (currentTouchOperation == TouchOperation.MOVE)
            {
                movingArrow.TouchDrag(touch);
            }
        }
        if (Input.touchCount == 2)
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
            return;
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
            return;
        }
	}
}
