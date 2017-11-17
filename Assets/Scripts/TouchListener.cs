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

    public VoxelEditorGUI editorGUI;
    public VoxelArray voxelArray;

    TouchOperation currentTouchOperation = TouchOperation.NONE;
    Transform pivot;
    Vector3 pivotRotationEuler;

    void Start()
    {
        pivot = transform.parent;
        pivotRotationEuler = pivot.rotation.eulerAngles;
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

            Voxel voxel;
            int faceI;

            if (currentTouchOperation == TouchOperation.NONE)
            {
                // wait until moved or released, in case a multitouch operation is about to begin
                if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Ended)
                {
                    Vector2 guiPos = touch.position;
                    guiPos.y = Screen.height - guiPos.y;
                    guiPos /= editorGUI.scaleFactor;

                    if (editorGUI.guiRect.Contains(guiPos))
                        currentTouchOperation = TouchOperation.GUI;
                    else if (!GetTouchSelection(out voxel, out faceI))
                    {
                        // ray hit nothing
                        voxelArray.SelectBackground();
                        currentTouchOperation = TouchOperation.NONE;
                    }
                    else if (voxel != null)
                    {
                        // ray hit a voxel
                        voxelArray.SelectDown(voxel, faceI);
                        currentTouchOperation = TouchOperation.SELECT;
                    }
                    else
                        // ray hit something other than a voxel, probably adjust axes or something
                        currentTouchOperation = TouchOperation.MOVE;
                }
            }

            if (currentTouchOperation == TouchOperation.GUI)
            {
                editorGUI.propertiesScroll.y += touch.deltaPosition.y / editorGUI.scaleFactor;
            }

            if (currentTouchOperation != TouchOperation.SELECT)
                return;

            GetTouchSelection(out voxel, out faceI);
            if (voxel != null)
            {
                voxelArray.SelectDrag(voxel, faceI);
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
            pivotRotationEuler.y += move.x * 0.3f;
            pivotRotationEuler.x -= move.y * 0.3f;
            if (pivotRotationEuler.x > 90)
                pivotRotationEuler.x = 90;
            if (pivotRotationEuler.x < -90)
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

    private bool GetTouchSelection(out Voxel voxel, out int faceI)
    {
        voxel = null;
        faceI = -1;
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.GetTouch(0).position), out hit))
        {
            if (hit.transform.gameObject.tag == "Voxel")
            {
                voxel = hit.transform.GetComponent<Voxel>();
                faceI = Voxel.FaceIForNormal(hit.normal);
                if (faceI == -1)
                    voxel = null;
            }
            return true;
        }
        else
        {
            return false;
        }
    }
}
