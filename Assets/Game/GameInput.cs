using UnityEngine;
#if UNITY_WEBGL
using System.Runtime.InteropServices;
#endif

public static class GameInput {
    public static Vector2 virtJoystick;
    public static bool virtJump;
    public static Vector2 virtLook;

#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern bool IsPointerLocked();
#else
    private static bool IsPointerLocked() {
        return Cursor.lockState == CursorLockMode.Locked;
    }
#endif

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
        } else if (IsPointerLocked()) {
            var mouseVec = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
#if UNITY_WEBGL && !UNITY_EDITOR
            mouseVec /= 2;
#endif
            return mouseVec;
        } else {
            return Vector2.zero;
        }
    }

    public static void LockCursor() {
        if (!UseTouchInput()) {
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public static void UnlockCursor() {
        Cursor.lockState = CursorLockMode.None;
    }
}
