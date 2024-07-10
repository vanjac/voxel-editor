using UnityEngine;

public static class GameInput {
    public static Vector2 virtJoystick;
    public static bool virtJump;

    public static bool UseTouchInput() {
#if UNITY_EDITOR
        return UnityEditor.EditorApplication.isRemoteConnected;
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
}
