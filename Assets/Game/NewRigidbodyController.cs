using System.Collections;
using System.Linq.Expressions;
using UnityEngine;
using UnityStandardAssets.Characters.FirstPerson;
using UnityStandardAssets.CrossPlatformInput;

public class NewRigidbodyController : MonoBehaviour
{
    private const float walkSpeed = 3.5f;
    private const float walkAccel = 6.0f;
    private const float stopDrag = 3.0f;
    private const float waterDrag = 5.0f;
    private const float stopDynamicFriction = 1.0f;
    private const float stopStaticFriction = 10.0f;
    private const float fallMoveSpeed = 1.5f;
    private const float jumpVel = 5.5f;
    private const float swimVel = 7.0f;
    public AnimationCurve slopeCurveModifier = new AnimationCurve(new Keyframe(-90.0f, 1.0f), new Keyframe(0.0f, 1.0f), new Keyframe(90.0f, 0.0f));

    private const float groundCheckDistance = 0.15f; // distance for checking if the controller is grounded ( 0.01f seems to work best for this )
    private const float stickToGroundHelperDistance = 0.6f; // stops the character
    private const float shellOffset = 0.1f; //reduce the radius by that ratio to avoid getting stuck in wall (a value of 0.1f is nice)
    private const float footstepStride = 1.0f;

    public MouseLook mouseLook = new MouseLook();

    public Camera cam;
    private Rigidbody rigidBody;
    private CapsuleCollider capsule;
    private FootstepSounds footstepSoundPlayer;
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
        Vector2 input = new Vector2(
            CrossPlatformInputManager.GetAxis("Horizontal"),
            CrossPlatformInputManager.GetAxis("Vertical"));
        bool hasInput = input.sqrMagnitude > 1e-12;

        GroundCheck(hasInput);
        bool underWater = false;
        PhysicsComponent physicsComponent = GetComponent<PhysicsComponent>();
        if (physicsComponent != null)
            underWater = physicsComponent.underWater;

        if (underWater)
        {
            capsule.material.dynamicFriction = 0.0f;
            capsule.material.staticFriction = 0.0f;
            capsule.material.frictionCombine = PhysicMaterialCombine.Minimum;
            rigidBody.drag = waterDrag;
        }
        else if (grounded && !hasInput && !jump)
        {
            capsule.material.dynamicFriction = stopDynamicFriction * SlopeMultiplier();
            capsule.material.staticFriction = stopStaticFriction * SlopeMultiplier();
            capsule.material.frictionCombine = PhysicMaterialCombine.Maximum;
            rigidBody.drag = stopDrag * SlopeMultiplier();
        }
        else
        {
            capsule.material.dynamicFriction = 0.0f;
            capsule.material.staticFriction = 0.0f;
            capsule.material.frictionCombine = PhysicMaterialCombine.Minimum;
            rigidBody.drag = 0;
        }

        float maxSpeed = (grounded && !underWater) ? walkSpeed : fallMoveSpeed;
        Vector3 desiredMove = Quaternion.AngleAxis(cam.transform.rotation.eulerAngles.y, Vector3.up)
            * new Vector3(input.x, 0, input.y);
        desiredMove *= input.magnitude * maxSpeed;
        if (hasInput &&
            (grounded || underWater || desiredMove.sqrMagnitude > rigidBody.velocity.sqrMagnitude))
        {
            Vector3 moveVector = desiredMove - rigidBody.velocity;
            moveVector = Vector3.ProjectOnPlane(moveVector, groundContactNormal).normalized;
            float maxVelChange = walkAccel * maxSpeed * Time.fixedDeltaTime;
            if (moveVector.sqrMagnitude > maxVelChange * maxVelChange)
                moveVector *= maxVelChange / moveVector.magnitude;
            rigidBody.AddForce(moveVector * SlopeMultiplier(), ForceMode.VelocityChange);
        }

        if (grounded || underWater)
        {
            if (jump)
            {
                rigidBody.velocity = new Vector3(rigidBody.velocity.x, 0f, rigidBody.velocity.z);
                rigidBody.AddForce(new Vector3(0f, underWater ? swimVel : jumpVel, 0f), ForceMode.VelocityChange);
                jumping = true;
            }
        }
        else
        {
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
    private void GroundCheck(bool hasInput)
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
            if (hasInput)
            {
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
            } // else rely on friction

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