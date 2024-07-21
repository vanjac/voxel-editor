using UnityEngine;
using UnityEngine.EventSystems;

public class GameTouchControl : MonoBehaviour {
    private const float DRAG_THRESHOLD = 64;
    private const float CARRY_DISTANCE = 3;
    private Camera cam;
    private int lookTouchId;
    private Vector2 lookTouchStart;
    private TapComponent touchedTapComponent;
    private CarryableComponent carriedComponent;
    public Joystick joystick;

    void Update() {
        // show/hide joystick and jump button
        bool touchEnabled = GameInput.UseTouchInput();
        foreach (Transform t in transform) {
            t.gameObject.SetActive(touchEnabled);
        }

        if (cam == null) {
            cam = Camera.main; // sometimes null for a few cycles
            if (cam != null && cam.tag == "DeathCamera") {
                cam = null;
            }
        }
        if (cam == null) {
            return; // sometimes null for a few cycles
        }

#if UNITY_EDITOR
        if (UnityEditor.EditorApplication.isRemoteConnected) {
#else
        if (Input.touchSupported) {
#endif
            UpdateTouchInput();
        } else {
            UpdateMouseInput();
        }
    }

    private void UpdateTouchInput() {
        bool setAxes = false;
        for (int i = 0; i < Input.touchCount; i++) {
            Touch touch = Input.GetTouch(i);
            if (touch.phase == TouchPhase.Began
                    && !EventSystem.current.IsPointerOverGameObject(touch.fingerId)
                    && GUIPanel.PanelContainingPoint(touch.position) == null) {
                lookTouchId = touch.fingerId;
                lookTouchStart = touch.position;
                TapStart(touch.position);
            }
            // don't move joystick and camera with same touch
            if (lookTouchId == joystick.dragTouchId) {
                lookTouchId = -1;
                TapEnd();
            }
            if (touch.fingerId == lookTouchId) {
                setAxes = true;
                // TODO: dependent on GameObject update order!
                GameInput.virtLook = touch.deltaPosition * 150f / cam.pixelHeight;
                if (touch.phase == TouchPhase.Moved || touch.phase == TouchPhase.Stationary) {
                    TapMove(touch.position);
                } else if (touch.phase == TouchPhase.Ended) {
                    TapEnd();
                }
                if (touch.phase == TouchPhase.Ended
                        && (touch.position - lookTouchStart).magnitude / GUIPanel.scaleFactor < DRAG_THRESHOLD) {
                    if (TapRaycast(touch.position, out var hit)) {
                        CarryableComponent hitCarryable = hit.transform.GetComponent<CarryableComponent>();
                        if (hitCarryable != null && hitCarryable.enabled) {
                            if (hitCarryable.IsCarried()) {
                                hitCarryable.Throw(PlayerComponent.instance);
                            } else if (hit.distance <= CARRY_DISTANCE) {
                                if (carriedComponent != null && carriedComponent.IsCarried()) {
                                    carriedComponent.Drop();
                                }
                                hitCarryable.Carry(PlayerComponent.instance);
                                carriedComponent = hitCarryable;
                            }
                        }
                    }
                }
            }  // end if touch.fingerId == lookTouchId
        }

        if (!setAxes) {
            lookTouchId = -1;
            GameInput.virtLook = Vector2.zero;
        }
    }

    private void UpdateMouseInput() {
        if (Input.GetMouseButtonDown(0)) {
            TapStart(Input.mousePosition);
        } else if (Input.GetMouseButtonUp(0)) {
            TapEnd();
        } else if (Input.GetMouseButton(0)) {
            TapMove(Input.mousePosition);
        }
    }

    private void TapStart(Vector2 position) {
        TapEnd();

        if (TapRaycast(position, out var hit)) {
            TapComponent hitTapComponent = hit.transform.GetComponent<TapComponent>();
            if (hitTapComponent != null && hit.distance <= hitTapComponent.Distance) {
                touchedTapComponent = hitTapComponent;
                touchedTapComponent.TapStart(PlayerComponent.instance);
            }
        }
    }

    private void TapEnd() {
        if (touchedTapComponent != null) {
            touchedTapComponent.TapEnd();
            touchedTapComponent = null;
        }
    }

    private void TapMove(Vector2 position) {
        if (touchedTapComponent == null) {
            // nothing
        } else if (TapRaycast(position, out var hit)) {
            // check for early cancel (tap left component)
            TapComponent hitTapComponent = hit.transform.GetComponent<TapComponent>();
            if (hitTapComponent != touchedTapComponent || hit.distance > touchedTapComponent.Distance) {
                TapEnd();
            }
        } else {
            TapEnd();
        }
    }

    private bool TapRaycast(Vector2 touchPosition, out RaycastHit hit) =>
        Physics.Raycast(cam.ScreenPointToRay(touchPosition), out hit,
            Mathf.Infinity, Physics.DefaultRaycastLayers,
            QueryTriggerInteraction.Ignore);
}
