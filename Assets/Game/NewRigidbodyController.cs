using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using UnityStandardAssets.CrossPlatformInput;

public class NewRigidbodyController : MonoBehaviour
{
    public float walkSpeed = 3.5f;
    public float fallMoveSpeed = 1.5f;
    public float jumpForce = 55f;
    public float swimForce = 70f;
    public AnimationCurve slopeCurveModifier = new AnimationCurve(new Keyframe(-90.0f, 1.0f), new Keyframe(0.0f, 1.0f), new Keyframe(90.0f, 0.0f));

    public float groundCheckDistance = 0.1f; // distance for checking if the controller is grounded ( 0.01f seems to work best for this )
    public float stickToGroundHelperDistance = 0.6f; // stops the character
    public float shellOffset = 0.1f; //reduce the radius by that ratio to avoid getting stuck in wall (a value of 0.1f is nice)
    public float footstepStride = 1.0f;

    public MouseLook mouseLook = new MouseLook();

    public Camera cam;
    private Rigidbody rigidBody;
    private CapsuleCollider capsule;
    private FootstepSounds footstepSoundPlayer;
    private float yRotation;
    private Vector3 groundContactNormal;
    private bool jump, previouslyGrounded, jumping, grounded;
    public bool disableGroundCheck;
    private bool previouslyUnderWater;
    private MaterialSound footstepSound;
    private float footstepDistance;
    private bool leftFoot;

    void Start()
    {
        rigidBody = GetComponent<Rigidbody>();
        capsule = GetComponent<CapsuleCollider>();
        footstepSoundPlayer = GetComponent<FootstepSounds>();
    }

    void Update()
    {
        RotateView();

        if (CrossPlatformInputManager.GetButtonDown("Jump") && !jump)
        {
            jump = true;
        }
    }

    void FixedUpdate()
    {
        GroundCheck();
        bool underWater = false;
        PhysicsComponent physicsComponent = GetComponent<PhysicsComponent>();
        if (physicsComponent != null)
            underWater = physicsComponent.underWater;

        Vector2 input = new Vector2
        {
            x = CrossPlatformInputManager.GetAxis("Horizontal"),
            y = CrossPlatformInputManager.GetAxis("Vertical")
        };

        if (Mathf.Abs(input.x) > float.Epsilon || Mathf.Abs(input.y) > float.Epsilon)
        {
            float maxSpeed = (grounded && !underWater) ? walkSpeed : fallMoveSpeed;
            // always move along the camera forward as it is the direction that it being aimed at
            Vector3 desiredMove = Quaternion.AngleAxis(cam.transform.rotation.eulerAngles.y, Vector3.up)
                * new Vector3(input.x, 0, input.y);
            desiredMove = Vector3.ProjectOnPlane(desiredMove, groundContactNormal).normalized;
            desiredMove *= input.magnitude * maxSpeed;
            if (rigidBody.velocity.sqrMagnitude < (maxSpeed * maxSpeed))
            {
                // TODO: scale by time??
                rigidBody.AddForce(desiredMove * SlopeMultiplier(), ForceMode.Impulse);
            }
        }

        if (grounded || underWater)
        {
            rigidBody.drag = 5f;

            if (jump)
            {
                rigidBody.drag = 0f;
                rigidBody.velocity = new Vector3(rigidBody.velocity.x, 0f, rigidBody.velocity.z);
                rigidBody.AddForce(new Vector3(0f, underWater ? swimForce : jumpForce, 0f), ForceMode.Impulse);
                jumping = true;
            }
        }
        else
        {
            rigidBody.drag = 0f;
            if (previouslyGrounded && !jumping)
            {
                StickToGroundHelper();
            }
        }

        // footstep sounds...
        if (underWater)
            footstepSound = MaterialSound.SWIM;
        if ((grounded || underWater) && jump)
        {
            PlayFootstep();
            footstepDistance = 0;
        }
        else if ((!previouslyGrounded && grounded && !underWater)
            || (!previouslyUnderWater && underWater))
        {
            // landed
            StartCoroutine(LandSoundCoroutine());
            footstepDistance = 0;
        }
        else if (grounded || underWater)
        {
            Vector3 velocity = rigidBody.velocity;
            velocity.y = 0;
            footstepDistance += velocity.magnitude * Time.fixedDeltaTime;
            if (footstepDistance > footstepStride)
            {
                footstepDistance -= footstepStride;
                PlayFootstep();
            }
        }

        jump = false;
        previouslyUnderWater = underWater;
    }

    private void PlayFootstep()
    {
        if (!PlayerComponent.instance.footstepSounds)
            return;
        if (leftFoot)
            footstepSoundPlayer.PlayLeftFoot(footstepSound);
        else
            footstepSoundPlayer.PlayRightFoot(footstepSound);
        leftFoot = !leftFoot;
    }

    private IEnumerator LandSoundCoroutine()
    {
        PlayFootstep();
        yield return new WaitForSeconds(1.0f / 30.0f);
        PlayFootstep();
    }


    private float SlopeMultiplier()
    {
        float angle = Vector3.Angle(groundContactNormal, Vector3.up);
        return slopeCurveModifier.Evaluate(angle);
    }


    private void StickToGroundHelper()
    {
        RaycastHit hitInfo;
        if (Physics.SphereCast(transform.position, capsule.radius * (1.0f - shellOffset), Vector3.down, out hitInfo,
                                ((capsule.height / 2f) - capsule.radius) +
                                stickToGroundHelperDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore))
        {
            if (Mathf.Abs(Vector3.Angle(hitInfo.normal, Vector3.up)) < 85f)
            {
                rigidBody.velocity = Vector3.ProjectOnPlane(rigidBody.velocity, hitInfo.normal);
            }
        }
    }


    private void RotateView()
    {
        //avoids the mouse looking if the game is effectively paused
        if (Mathf.Abs(Time.timeScale) < float.Epsilon) return;

        // get the rotation before it's changed
        float oldYRotation = transform.eulerAngles.y;

        mouseLook.LookRotation(transform, cam.transform);

        // Rotate the rigidbody velocity to match the new direction that the character is looking
        Quaternion velRotation = Quaternion.AngleAxis(transform.eulerAngles.y - oldYRotation, Vector3.up);
        rigidBody.velocity = velRotation * rigidBody.velocity;
    }

    /// sphere cast down just beyond the bottom of the capsule to see if the capsule is colliding round the bottom
    private void GroundCheck()
    {
        previouslyGrounded = grounded;
        if (disableGroundCheck)
        {
            disableGroundCheck = false;
            grounded = false;
            jumping = true;
        }

        RaycastHit hitInfo;
        if (Physics.SphereCast(transform.position, capsule.radius * (1.0f - shellOffset), Vector3.down, out hitInfo,
                                ((capsule.height / 2f) - capsule.radius) + groundCheckDistance, Physics.AllLayers, QueryTriggerInteraction.Ignore))
        {
            grounded = true;
            groundContactNormal = hitInfo.normal;
            // move with moving object
            Vector3 move = Vector3.zero;
            foreach (IMotionComponent motionComponent in hitInfo.transform.GetComponents<IMotionComponent>())
            {
                if (motionComponent.enabled)
                {
                    move += motionComponent.GetTranslateFixed();
                    Vector3 relPos = transform.position - motionComponent.transform.position;
                    move += (motionComponent.GetRotateFixed() * relPos) - relPos;
                }
            }
            move.y = 0;
            if (move != Vector3.zero)
                rigidBody.MovePosition(rigidBody.position + move);

            // determine footstep sound
            if (hitInfo.collider.gameObject.tag == "Voxel")
            {
                var voxelComponent = hitInfo.collider.GetComponent<VoxelComponent>();
                Voxel voxel;
                int faceI;
                if (voxelComponent.GetSubstance() != null)
                {
                    // substances use convex hulls which don't have submeshes
                    // so just use the top face of the voxel
                    voxel = voxelComponent.GetVoxelForCollider(hitInfo.collider);
                    faceI = 3;
                }
                else
                {
                    int hitVertexI = TouchListener.GetRaycastHitVertexIndex(hitInfo);
                    voxelComponent.GetVoxelFaceForVertex(hitVertexI, out voxel, out faceI);
                }
                if (voxel != null)
                    footstepSound = voxel.faces[faceI].GetSound();
                else
                    footstepSound = MaterialSound.GENERIC;
            }
            else
            {
                Renderer hitRender = hitInfo.collider.GetComponent<Renderer>();
                if (hitRender != null)
                    // regular .material has (Instance) suffix
                    footstepSound = ResourcesDirectory.GetMaterialSound(hitRender.sharedMaterial);
                else
                    footstepSound = MaterialSound.GENERIC;
            }
        }
        else
        {
            grounded = false;
            groundContactNormal = Vector3.up;
        }
        if (!previouslyGrounded && grounded && jumping)
        {
            jumping = false;
        }
    }
}