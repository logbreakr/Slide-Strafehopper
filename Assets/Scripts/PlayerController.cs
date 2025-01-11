using System.Xml;
using Unity.Burst;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{
    private ConstantForce cf;

    private float horizontalInput, verticalInput;
    private Vector3 wishVector = Vector3.zero;

    [SerializeField]
    private float groundSpeed,
        walkSpeed = 11f,
        crouchSpeed = 4f,
        groundAccel = 75f,
        groundMaxAccel = 100f,
        maxSlope = 45.573f,
        gravity,
        groundedDist = 0.6f;
    [SerializeField]
    float airAccel = 5f;

    [SerializeField]
    private PhysicsMaterial kineticPhysicsMaterial;

    // movement
    Rigidbody rb;
    [SerializeField]
    public bool isSliding = false,
        isCrouching = false;


    [SerializeField]
    GameObject headBone;
    [SerializeField]
    GameObject headTarget;
    [SerializeField]
    float camSmoothing = 4f;
    Vector3 camInitialOffset = Vector3.zero;
    [SerializeField]
    GameObject body;

    private float kineticFriction,
        slopeFrictionModifier = 0.15f,
        slideThresh,
        minSlideThresh;
    private float slideBoostTimer,
        slideBoostTimerMax = 2f,
        slideBoostVel = 15f;
    private bool slideBoostCount = false;
    [SerializeField]
    private AnimationCurve minSlideAngleVelocity, accelFactorFromDot;
    private float lowFricSlopeAng = 30f;

    //grounding
    int stepsSinceLastGrounded, stepsSinceLastJump;

    public LayerMask layerMask;

   [SerializeField]
    private float jumpForce = 5;
    float jumpBuffer = 0.1f;

    // mouse variables
    private Vector2 rotateTotal;
    [SerializeField]
    private float mouseSpeed = 50f,
        vertRange = 90;
    [SerializeField]
    private GameObject cam;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        cf = GetComponent<ConstantForce>();
        UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;
        groundSpeed = walkSpeed;
        gravity = Physics.gravity.magnitude;
        cf.force = -Vector3.up * gravity * rb.mass;
        slideBoostTimer = 0f;
        camInitialOffset = cam.transform.position - GameObject.Find("spine.006_end").transform.position;
    }

    void Update()
    {
        // inputs
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");

        rotateTotal.x += Input.GetAxis("Mouse X") * mouseSpeed;
        rotateTotal.y += Input.GetAxis("Mouse Y") * mouseSpeed;

        rotateTotal.y = Mathf.Clamp(rotateTotal.y, -vertRange, vertRange);

        cam.transform.rotation = Quaternion.Euler(-rotateTotal.y, rotateTotal.x, 0);

        // make cam holder follow headbone with y lerped
        Vector3 camOffset = headBone.transform.position + (Quaternion.AngleAxis(rotateTotal.x, Vector3.up)) * camInitialOffset;
        cam.transform.position = camOffset;
        cam.transform.position = new Vector3(cam.transform.position.x, Vector3.Lerp(cam.transform.position, headBone.transform.position, camSmoothing * Time.deltaTime).y, cam.transform.position.z);


        // set wish vector, set length to 1
        wishVector.Set(horizontalInput, 0, verticalInput);
        wishVector.Normalize();

        Jump();
        Crouch();

        if (slideBoostCount)
        {
            slideBoostTimer -= Time.deltaTime;

            if (slideBoostTimer < 0)
            {
                slideBoostCount = false;
                slideBoostTimer = 0f;
            }
        }
        //Debug.Log(rb.linearVelocity.magnitude);
    }


    private void FixedUpdate()
    {
        UpdateState();

        wishVector = Quaternion.AngleAxis(cam.transform.eulerAngles.y, Vector3.up) * wishVector;

        if (IsGrounded(true, groundedDist))
        {
            MovePlayer();
        }
        else
        {
            AirAccelerate(wishVector);
        }

        StaticFriction();
        StickToGround();
    }

    public void UpdateState()
    {
        stepsSinceLastGrounded += 1;
        stepsSinceLastJump += 1;


        if (IsGrounded(false, groundedDist))
        {
            stepsSinceLastGrounded = 0;
        }
    }

    public bool IsGrounded(bool var, float dist)
    {
        bool ground = Physics.SphereCast(transform.position, 0.5f, -Vector3.up, out RaycastHit hit, dist, layerMask);
        if (var)
        {
            if (Vector3.Angle(Normal(rb.position), Vector3.up) < maxSlope && !isSliding)
            {
                return ground;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return ground;
        }
    }

    public Vector3 Normal(Vector3 position)
    {
        RaycastHit hit;
        if (Physics.Raycast(position, -Vector3.up, out hit, Mathf.Infinity, layerMask))
        {
            return hit.normal;
        }
        else
        {
            return Vector3.zero;
        }
    }

    public void MovePlayer()
    {
        // WASD movement
        Vector3 towardVelocity = rb.linearVelocity;
        Vector3 neededAccel = Vector3.zero;

        Vector3 goalVelocity = Vector3.ProjectOnPlane(wishVector, Normal(rb.position)).normalized * groundSpeed;

        float velDot = Vector3.Dot(wishVector, rb.linearVelocity.normalized);
        float forceScale = accelFactorFromDot.Evaluate(velDot);

        towardVelocity = Vector3.MoveTowards(towardVelocity, goalVelocity + ParentVelocity(), groundAccel);

        neededAccel = (towardVelocity - rb.linearVelocity) / Time.fixedDeltaTime;
        neededAccel = Vector3.ClampMagnitude(neededAccel, groundMaxAccel * forceScale);

        rb.AddForce(Vector3.ProjectOnPlane(neededAccel * rb.mass * forceScale, Normal(rb.position)));

        Physics.SyncTransforms();
    }

    public Vector3 ParentVelocity()
    {
        if (transform.parent != null)
        {
            return transform.parent.GetComponent<Rigidbody>().GetPointVelocity(transform.position);
        }
        else
        {
            return Vector3.zero;
        }
    }
    public void Jump()
    {
        if (IsGrounded(false, groundedDist) && Input.GetKeyDown(KeyCode.Space))
        {
            stepsSinceLastJump = 0;
            if (rb.linearVelocity.y < 0)
            {
                rb.AddForce(Vector3.up * (jumpForce + Mathf.Abs(rb.linearVelocity.y)) * rb.mass, ForceMode.Impulse);
            }
            else
            {
                rb.AddForce(Vector3.up * jumpForce * rb.mass, ForceMode.Impulse);
            }
        }
        jumpBuffer -= Time.deltaTime;
        if (jumpBuffer < 0)
        {
            jumpBuffer = 0;
        }
        if (IsGrounded(true, groundedDist) && jumpBuffer == 0 && !isCrouching)
        {
            jumpBuffer = 0.1f;
        }
    }
    public void StickToGround()
    {
        if (stepsSinceLastGrounded > 1 || stepsSinceLastJump <= 3 || IsGrounded(false, groundedDist))
        {
            return;
        }
        rb.AddForce(-Normal(rb.position + (rb.linearVelocity.normalized * 0.1f)) * Vector3.Project(rb.linearVelocity, Normal(rb.position + (rb.linearVelocity.normalized * 0.1f))).magnitude * rb.mass, ForceMode.Impulse);
    }

    public void AirAccelerate(Vector3 wishVector)
    {
        float addSpeed, wishSpeed, accelSpeed, currentSpeed;
        Vector3 wishVelocity = Vector3.zero;

        wishSpeed = wishVector.magnitude;
        wishVelocity = Vector3.Normalize(wishVector);

        if (wishSpeed > 30)
        {
            wishSpeed = 30;
        }

        currentSpeed = Vector3.Dot(rb.linearVelocity, wishVelocity);
        addSpeed = wishSpeed - currentSpeed;
        if (addSpeed <= 0)
        {
            return;
        }

        accelSpeed = groundSpeed * airAccel * Time.fixedDeltaTime;
        if (accelSpeed > addSpeed)
        {
            accelSpeed = addSpeed;
        }

        rb.linearVelocity += new Vector3(accelSpeed * wishVelocity.x, 0, accelSpeed * wishVelocity.z);
    }

    public void Crouch()
    {
        // TODO FIX KEYBINDS WITH NEW INPUT MANAGER

        // enter crouch
        if (Input.GetKeyDown(KeyCode.C))
        {
            isCrouching = true;
            groundSpeed = crouchSpeed;
        }
        // exit crouch
        if (Input.GetKeyUp(KeyCode.C))
        {
            isCrouching = false;
            groundSpeed = walkSpeed;
        }

        if (!MovingUpSlope())
        {
            slideThresh = minSlideAngleVelocity.Evaluate(Vector3.Angle(Normal(rb.position), Vector3.up));
        }
        else
        {
            slideThresh = minSlideAngleVelocity.Evaluate(0);
        }
        minSlideThresh = Mathf.Min(crouchSpeed, slideThresh);
        
        // set sliding
        if ((Vector3.ProjectOnPlane(rb.linearVelocity - ParentVelocity(), Normal(rb.position)).magnitude > slideThresh) && isCrouching && !isSliding)
        {
            isSliding = true;

            if (slideBoostTimer == 0)
            {
                slideBoostTimer = slideBoostTimerMax;
                slideBoostCount = true;

                Vector3 towardVelocity = rb.linearVelocity;
                towardVelocity = (towardVelocity.normalized * slideBoostVel) + ParentVelocity() - rb.linearVelocity;
                rb.AddForce(Vector3.ProjectOnPlane(towardVelocity * rb.mass, Normal(rb.position)), ForceMode.Impulse);
            }

        }

        if (Vector3.ProjectOnPlane(rb.linearVelocity - ParentVelocity(), Normal(rb.position)).magnitude < minSlideThresh || !isCrouching)
        {
            isSliding = false;
        }
        // sliding friction force
        if (isSliding && IsGrounded(false, groundedDist))
        {
            KineticFriction();
        }
    }

    public bool MovingUpSlope()
    {
        Vector3 left = Vector3.Cross(Normal(rb.position), Vector3.up);
        Vector3 slopeUp = -Vector3.Cross(Normal(rb.position), left);
        return Vector3.Dot(Vector3.ProjectOnPlane(rb.linearVelocity, Normal(rb.position)).normalized, slopeUp.normalized) > 0;
    }
    public void KineticFriction()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, -Vector3.up, out hit, Mathf.Infinity, layerMask))
        {
            kineticFriction = hit.transform.GetComponent<Collider>().material.dynamicFriction;
            if (Mathf.Round(Vector3.Angle(Normal(rb.position), Vector3.up)) >= lowFricSlopeAng && !MovingUpSlope())
            {
                kineticFriction -= slopeFrictionModifier;
            }
        }
        Vector3 frictionForce = (ParentVelocity() - rb.linearVelocity).normalized * (kineticFriction * rb.mass * gravity * Mathf.Cos((Vector3.Angle(Normal(rb.position), Vector3.up)) * Mathf.PI / 180f));
        rb.AddForce(frictionForce);
    }

    public void StaticFriction()
    {
        // orient player gravity (constant force) into normal of slope to prevent sliding
        if (wishVector == Vector3.zero && IsGrounded(true, groundedDist) && rb.linearVelocity.magnitude < 0.25f)
        {
            cf.force = Vector3.Project(Physics.gravity * rb.mass, -Normal(rb.position));
        }
        else
        {
            cf.force = -Vector3.up * gravity * rb.mass;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        transform.rotation = Quaternion.Euler(Vector3.up);
        Physics.SyncTransforms();
    }
}
