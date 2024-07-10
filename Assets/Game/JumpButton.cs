using UnityEngine;
using UnityEngine.EventSystems;

public class JumpButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
    public void OnPointerDown(PointerEventData data) {
        GameInput.virtJump = true;
    }

    public void OnPointerUp(PointerEventData data) {
        GameInput.virtJump = false;
    }
}
