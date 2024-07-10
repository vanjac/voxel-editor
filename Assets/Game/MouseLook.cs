using System;
using UnityEngine;

// modified from Unity standard assets!

namespace UnityStandardAssets.Characters.FirstPerson {
    [Serializable]
    public class MouseLook {
        public float XSensitivity = 2f;
        public float YSensitivity = 2f;
        public bool clampVerticalRotation = true;
        public float MinimumX = -90F;
        public float MaximumX = 90F;
        public float smoothTime = 5f;

        public void LookRotation(Transform character, Transform camera) {
            var look = GameInput.GetLookDelta();

            character.localRotation *= Quaternion.Euler(0f, look.x * XSensitivity, 0f);
            camera.localRotation *= Quaternion.Euler(-look.y * YSensitivity, 0f, 0f);

            if (clampVerticalRotation) {
                camera.localRotation = ClampRotationAroundXAxis(camera.localRotation);
            }
        }

        Quaternion ClampRotationAroundXAxis(Quaternion q) {
            q.x /= q.w;
            q.y /= q.w;
            q.z /= q.w;
            q.w = 1.0f;

            float angleX = 2.0f * Mathf.Rad2Deg * Mathf.Atan(q.x);

            angleX = Mathf.Clamp(angleX, MinimumX, MaximumX);

            q.x = Mathf.Tan(0.5f * Mathf.Deg2Rad * angleX);

            return q;
        }

    }
}
