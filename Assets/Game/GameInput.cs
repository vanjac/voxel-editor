using UnityEngine;

public static class GameInput {
    public static Vector2 virtJoystick;
    public static bool virtJump;
    public static Vector2 virtLook;

    public static bool UseTouchInput() {
#if UNITY_EDITOR
        return UnityEditor.EditorApplication.isRemoteConnected;
#elif UNITY_WEBGL
        return false;
#else
        return Input.touchSupported;
#endif
    }

    public static Vector2 GetJoystick() {
        if (UseTouchInput()) {
            return virtJoystick;
        } else {
            return new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        }
    }

    public static bool GetJump() => UseTouchInput() ? virtJump : Input.GetButton("Jump");

    public static Vector2 GetLookDelta() {
        if (UseTouchInput()) {
            return virtLook;
        } else if (Cursor.lockState == CursorLockMode.Locked) {
            return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        } else {
            return Vector2.zero;
        }
    }

    public static void LockCursor() {
        if (!UseTouchInput()) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    public static void UnlockCursor() {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}
