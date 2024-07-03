using UnityEngine;

public enum VoxelElement {
    FACES, EDGES
}

public class TouchListener : MonoBehaviour {
    private const float MAX_ZOOM = 20.0f;
    private const float MIN_ZOOM = .05f;
    private const int NO_XRAY_MASK = Physics.DefaultRaycastLayers & ~(1 << 8); // everything but XRay layer
    private const int NO_TRANSPARENT_MASK = NO_XRAY_MASK & ~(1 << 10); // everything but XRay and TransparentObject

    public enum CameraMode {
        ORBIT, PAN
    }
    public enum TouchOperation {
        NONE, SELECT, CAMERA, GUI, MOVE
    }

    public VoxelArrayEditor voxelArray;

    public CameraMode cameraMode;
    public TouchOperation currentTouchOperation = TouchOperation.NONE;
    public VoxelElement selectType = VoxelElement.FACES;
    public TransformAxis movingAxis;
    public Transform pivot;
    private Camera cam;

    private Vector3Int lastHitPosition = VoxelArray.NONE;
    private int lastHitElementI;
    private bool selectingXRay = true;
    private Vector2 lastMousePos;

    void Start() {
        pivot = transform.parent;
        cam = GetComponent<Camera>();
        lastMousePos = Input.mousePosition;
    }

    void Update() {
        bool touchSupported;
#if UNITY_EDITOR
        touchSupported = UnityEditor.EditorApplication.isRemoteConnected;
#else
        touchSupported = Input.touchSupported;
#endif

        if (Input.touchCount == 1 || (!touchSupported && Input.GetMouseButton(0))) {
            Touch touch;
            if (touchSupported) {
                touch = Input.GetTouch(0);
            } else {
                touch = new Touch();
                touch.position = Input.mousePosition;
                touch.deltaPosition = touch.position - lastMousePos;
                touch.phase = TouchPhase.Moved;
                if (Input.GetMouseButtonDown(0)) {
                    touch.phase = TouchPhase.Began;
                }
                if (Input.GetMouseButtonUp(0)) {
                    touch.phase = TouchPhase.Ended;
                }
                touch.tapCount = 1;
            }

            if (currentTouchOperation != TouchOperation.SELECT) {
                selectingXRay = true;
            }
            bool rayHitSomething = Physics.Raycast(cam.ScreenPointToRay(touch.position),
                out RaycastHit hit, Mathf.Infinity, selectingXRay ? Physics.DefaultRaycastLayers : NO_XRAY_MASK);
            Vector3Int hitPosition = VoxelArray.NONE;
            int hitElementI = -1;
            TransformAxis hitTransformAxis = null;
            ObjectMarker hitMarker = null;
            if (rayHitSomething) {
                GameObject hitObject = hit.transform.gameObject;
                if (hitObject.tag == "Voxel") {
                    var voxelComponent = hitObject.GetComponent<VoxelComponent>();
                    int hitVertexI = GetRaycastHitVertexIndex(hit);
                    var hitFace = voxelComponent.GetVoxelFaceForVertex(hitVertexI);
                    if (selectType == VoxelElement.FACES) {
                        hitElementI = hitFace.faceI;
                    } else if (selectType == VoxelElement.EDGES) {
                        var hitVoxel = voxelArray.VoxelAt(hitFace.position, false);
                        hitElementI = ClosestEdgeToUV(hitVoxel, hit.textureCoord, hitFace.faceI);
                    } else {
                        hitElementI = -1;
                    }
                    if (hitElementI != -1) {
                        hitPosition = hitFace.position;
                    }
                } else if (hitObject.tag == "ObjectMarker") {
                    if (selectType != VoxelElement.EDGES) {
                        hitMarker = hitObject.GetComponentInParent<ObjectMarker>();
                    }
                } else if (hitObject.tag == "MoveAxis") {
                    hitTransformAxis = hitObject.GetComponent<TransformAxis>();
                }

                var hitSubstance = voxelArray.SubstanceAt(hitPosition);
                if ((hitSubstance != null && hitSubstance.xRay)
                        || (hitMarker != null && (hitMarker.gameObject.layer == 8 || hitMarker.gameObject.layer == 10))) { // xray or TransparentObject layer
                    // allow moving axes through xray substances
                    if (Physics.Raycast(cam.ScreenPointToRay(touch.position),
                            out RaycastHit newHit, Mathf.Infinity, NO_TRANSPARENT_MASK)) {
                        if (newHit.transform.tag == "MoveAxis") {
                            hitPosition = VoxelArray.NONE;
                            hitElementI = -1;
                            hitMarker = null;
                            hitTransformAxis = newHit.transform.GetComponent<TransformAxis>();
                        }
                    }
                }
            }
            if (hitPosition != VoxelArray.NONE) {
                lastHitPosition = hitPosition;
                lastHitElementI = hitElementI;
            } else if (hitMarker != null) {
                lastHitPosition = VoxelArray.NONE;
                lastHitElementI = -1;
            }

            if (touch.phase == TouchPhase.Began) {
                if (currentTouchOperation == TouchOperation.SELECT) {
                    voxelArray.TouchUp();
                }
                // this seems to improve the reliability of double-taps when things are running slowly.
                // I think it's because there's not always a long enough gap between taps
                // for the touch operation to be cleared.
                currentTouchOperation = TouchOperation.NONE;
            }

            if (currentTouchOperation == TouchOperation.NONE) {
                if (GUIPanel.PanelContainingPoint(touch.position) != null) {
                    currentTouchOperation = TouchOperation.GUI;
                } else if (touch.phase != TouchPhase.Moved && touch.phase != TouchPhase.Ended && touch.tapCount == 1) {
                    // wait until moved or released, in case a multitouch operation is about to begin
                } else if (!rayHitSomething) {
                    voxelArray.TouchDown(null);
                } else if (hitPosition != VoxelArray.NONE) {
                    if (touch.tapCount == 1) {
                        currentTouchOperation = TouchOperation.SELECT;
                        voxelArray.TouchDown(hitPosition, hitElementI, selectType);
                        selectingXRay = voxelArray.SubstanceAt(hitPosition)?.xRay ?? false;
                    } else if (touch.tapCount == 2 && touch.phase == TouchPhase.Began) {
                        voxelArray.DoubleTouch(hitPosition, hitElementI, selectType);
                    } else if (touch.tapCount == 3 && touch.phase == TouchPhase.Began) {
                        voxelArray.TripleTouch(hitPosition, hitElementI, selectType);
                    }
                    UpdateZoomDepth();
                } else if (hitMarker != null) {
                    currentTouchOperation = TouchOperation.SELECT;
                    voxelArray.TouchDown(hitMarker);
                    selectingXRay = hitMarker.objectEntity.xRay;
                    UpdateZoomDepth();
                } else if (hitTransformAxis != null) {
                    if (touch.tapCount == 1) {
                        currentTouchOperation = TouchOperation.MOVE;
                        movingAxis = hitTransformAxis;
                        movingAxis.TouchDown(touch);
                    } else if (touch.tapCount == 2 && touch.phase == TouchPhase.Began && lastHitPosition != VoxelArray.NONE) {
                        voxelArray.DoubleTouch(lastHitPosition, lastHitElementI, selectType);
                    } else if (touch.tapCount == 3 && touch.phase == TouchPhase.Began && lastHitPosition != VoxelArray.NONE) {
                        voxelArray.TripleTouch(lastHitPosition, lastHitElementI, selectType);
                    }
                    UpdateZoomDepth();
                }
            } else if (currentTouchOperation == TouchOperation.SELECT) {
                if (hitPosition != VoxelArray.NONE) {
                    voxelArray.TouchDrag(hitPosition, hitElementI, selectType);
                }
                if (hitMarker != null) {
                    voxelArray.TouchDrag(hitMarker);
                }
            } else if (currentTouchOperation == TouchOperation.MOVE) {
                movingAxis.TouchDrag(touch);
            }
        } else if (Input.touchCount == 2) {
            if (currentTouchOperation == TouchOperation.NONE) {
                currentTouchOperation = TouchOperation.CAMERA;
                UpdateZoomDepth();
            }
            if (currentTouchOperation != TouchOperation.CAMERA) {
                return;
            }

            Touch touchZero = Input.GetTouch(0);
            Touch touchOne = Input.GetTouch(1);

            Vector2 touchZeroPrevPos = touchZero.position - touchZero.deltaPosition;
            Vector2 touchOnePrevPos = touchOne.position - touchOne.deltaPosition;

            float prevTouchDeltaMag = (touchZeroPrevPos - touchOnePrevPos).magnitude;
            float touchDeltaMag = (touchZero.position - touchOne.position).magnitude;

            float deltaMagnitudeDiff = prevTouchDeltaMag - touchDeltaMag;

            float scaleFactor = Mathf.Pow(1.005f, deltaMagnitudeDiff / cam.pixelHeight * 700f);
            if (scaleFactor != 1) {
                pivot.localScale *= scaleFactor;
                if (pivot.localScale.x > MAX_ZOOM) {
                    pivot.localScale = new Vector3(MAX_ZOOM, MAX_ZOOM, MAX_ZOOM);
                }
                if (pivot.localScale.x < MIN_ZOOM) {
                    pivot.localScale = new Vector3(MIN_ZOOM, MIN_ZOOM, MIN_ZOOM);
                }
            }

            Vector2 move = (touchZero.deltaPosition + touchOne.deltaPosition) / 2;
            if (cameraMode == CameraMode.ORBIT) {
                Orbit(move);
            } else if (cameraMode == CameraMode.PAN) {
                Pan(move);
            }
        } else if (Input.touchCount == 3) {
            if (currentTouchOperation == TouchOperation.NONE) {
                currentTouchOperation = TouchOperation.CAMERA;
                UpdateZoomDepth();
            }
            if (currentTouchOperation != TouchOperation.CAMERA) {
                return;
            }

            Vector2 move = Vector2.zero;
            for (int i = 0; i < 3; i++) {
                move += Input.GetTouch(i).deltaPosition;
            }
            move /= 3;
            if (cameraMode == CameraMode.ORBIT) {
                Pan(move); // reverse of 2 touch
            } else if (cameraMode == CameraMode.PAN) {
                Orbit(move);
            }
        } else { // no touch / more than 3
            if (currentTouchOperation == TouchOperation.SELECT) {
                voxelArray.TouchUp();
            }
            if (currentTouchOperation == TouchOperation.MOVE) {
                movingAxis.TouchUp();
            }
            currentTouchOperation = TouchOperation.NONE;
        }
        lastMousePos = Input.mousePosition;
    }

    private void Orbit(Vector2 move) {
        move *= 300f;
        move /= cam.pixelHeight;
        Vector3 pivotRotationEuler = pivot.rotation.eulerAngles;
        pivotRotationEuler.y += move.x;
        pivotRotationEuler.x -= move.y;
        if (pivotRotationEuler.x > 90 && pivotRotationEuler.x < 180) {
            pivotRotationEuler.x = 90;
        }
        if (pivotRotationEuler.x < -90 || (pivotRotationEuler.x > 180 && pivotRotationEuler.x < 270)) {
            pivotRotationEuler.x = -90;
        }
        pivot.rotation = Quaternion.Euler(pivotRotationEuler);
    }

    private void Pan(Vector2 move) {
        move *= 12.0f;
        move /= cam.pixelHeight;
        pivot.position -= move.x * pivot.right * pivot.localScale.z;
        pivot.position -= move.y * pivot.up * pivot.localScale.z;
    }

    private void UpdateZoomDepth() {
        // adjust the depth of the pivot point to the depth at the average point between the fingers
        int touchCount = Input.touchCount;
        Vector2 avg = Vector2.zero;
        for (int i = 0; i < touchCount; i++) {
            avg += Input.GetTouch(i).position;
        }
        if (touchCount > 0) {
            avg /= touchCount;
        } else {
            avg = new Vector2(cam.pixelWidth / 2, cam.pixelHeight / 2);
        }

        Ray ray = cam.ScreenPointToRay(avg);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, NO_XRAY_MASK)) {
            float currentDistanceToCamera = (pivot.position - transform.position).magnitude;
            float newDistanceToCamera = (hit.point - transform.position).magnitude;
            pivot.position = (pivot.position - transform.position).normalized * newDistanceToCamera + transform.position;
            pivot.localScale *= newDistanceToCamera / currentDistanceToCamera;
        }
    }

    // the first vertex of the triangle that was hit
    public static int GetRaycastHitVertexIndex(RaycastHit hit) {
        var mesh = ((MeshCollider)(hit.collider)).sharedMesh;
        return mesh.triangles[hit.triangleIndex * 3];
    }

    private static int ClosestEdgeToUV(Voxel voxel, Vector2 uv, int faceI) {
        if (voxel == null) {
            return -1;
        }
        float minDist = 1.0f;
        int closestEdge = -1;
        int n = 0;
        foreach (int edgeI in Voxel.FaceSurroundingEdges(faceI)) {
            float dist = 2.0f;
            switch (voxel.FaceTransformedEdgeNum(faceI, n)) {
                case 0:
                    dist = uv.y - Mathf.Floor(uv.y);
                    break;
                case 1:
                    dist = Mathf.Ceil(uv.x) - uv.x;
                    break;
                case 2:
                    dist = Mathf.Ceil(uv.y) - uv.y;
                    break;
                case 3:
                    dist = uv.x - Mathf.Floor(uv.x);
                    break;
            }
            if (dist < minDist) {
                minDist = dist;
                closestEdge = edgeI;
            }
            n++;
        }
        return closestEdge;
    }
}
