using System.Collections;
using UnityEngine;

public class Movement : MonoBehaviour
{
    [Header("REFERENCE")]
    public Animator animator;
	EnemyBehavior eb;

	[Header("Movement")]
    private float moveSpeed;
    public float walkSpeed;
    public float sprintSpeed;
    public float groundDrag;
    public float wallRunSpeed;

    [Header("Jumping")]
    public float jumpForce;
    public float jumpCooldown;
    public float airMultiplier;
    public float airDampen = 0.94f;
    private bool readyToJump = true;

    [Header("Crouching")]
    public float crouchSpeed;
    public float crouchYScale;
    private float startYScale;

    [Header("Keybinds")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public KeyCode crouchKey = KeyCode.LeftControl;

    [Header("SLASH")]
    public UnityEngine.Vector3 boxSize = new UnityEngine.Vector3(1f, 1f, 1f);
    public UnityEngine.Vector3 boxDirection = UnityEngine.Vector3.forward;
	public LayerMask slashLayer;
    public float slashDistance;
    public float slashDamage;

	[Header("Ground Check")]
    public float height;
    public LayerMask whatIsGround;
    public bool grounded;

    public Transform orientation;
    public bool player1;

    [Header("Slope Handling")]
    public float maxSlopeAngle;
    private RaycastHit slopeHit; 
    private bool exitingSlope;

    [Header("Monitor")]
    [SerializeField] public float horizontalInput;
    [SerializeField] public float verticalInput;
    [SerializeField] private GameManager gameManager;
    public bool isGrappleAvailable;

    private UnityEngine.Vector3 moveDirection;
    private Rigidbody rb;
    public MovementState state;

	private Coroutine forceCoroutine;

	public enum MovementState
    {
        freeze,
        grappling,
        walking,
        sprinting,
        air,
        crouching,
        wallrunning
    }

    public bool wallrunning;
    public bool freeze; 
    public bool activeGrapple;

    // Initializes the Rigidbody and other components at the start of the game
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
         
        gameManager = FindFirstObjectByType<GameManager>();

		startYScale = transform.localScale.y;

        isGrappleAvailable = false;
	}

    // Checks for grounded status and handles player movement input every frame
    void Update()
    {
        grounded = Physics.Raycast(transform.position, UnityEngine.Vector3.down, height * 0.5f + 0.2f, whatIsGround);

        // Checks if the player is the active player based on the camera in the game manager
        if (player1 == gameManager.getCamera() && gameManager.getLock())
        {
            MyInput();
            SpeedControl();
            StateHandler();
        }

        if (grounded && !activeGrapple)
            rb.linearDamping = groundDrag;
        else
            rb.linearDamping = 0;

        if (player1 != gameManager.getCamera())
        {
            rb.linearDamping = groundDrag * 0.5f;
            horizontalInput = 0;
            verticalInput = 0;
        }

		//DebugDrawBoxCast(transform.position, boxSize, boxDirection, slashDistance, Color.blue);
	}

    // Handles the physics-based movement of the player
    void FixedUpdate()
    {
        MovePlayer();
    }

    // Captures player input for movement, jumping, and crouching
    void MyInput()
    {
        verticalInput = Input.GetAxisRaw("vertical");
        horizontalInput = Input.GetAxisRaw("horizontal");

        // Checks if the jump key is pressed, and if the player is ready to jump and grounded
        if (Input.GetKey(jumpKey) && readyToJump && (grounded || wallrunning))
        {
            readyToJump = false; // Marking the player as not ready to jump again
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown); // Resets jump availability after cooldown
        }

        // Checks if the crouch key is pressed to modify player scale and apply force
        if (Input.GetKeyDown(crouchKey) && grounded)
        {
            transform.localScale = new UnityEngine.Vector3(transform.localScale.x, crouchYScale, transform.localScale.z);
            rb.AddForce(UnityEngine.Vector3.down * 5f, ForceMode.Impulse);
        }

        // Checks if the crouch key is released to restore original scale
        if (Input.GetKeyUp(crouchKey))
        {
            transform.localScale = new UnityEngine.Vector3(transform.localScale.x, startYScale, transform.localScale.z);
        }
    }

    // Determines the current movement state based on player input and grounded status
    private void StateHandler()
    {
        if (freeze)
        {
            state = MovementState.freeze;
            moveSpeed = 0;
            rb.linearVelocity = UnityEngine.Vector3.zero;
        }
        else if (activeGrapple)
        {
            state = MovementState.grappling;
            moveSpeed = sprintSpeed;
        }
        else if (wallrunning)
        {
            state = MovementState.wallrunning;
            moveSpeed = wallRunSpeed;
        }
        else if (grounded && Input.GetKey(crouchKey))
        {
            state = MovementState.crouching;
            moveSpeed = crouchSpeed; // Set movement speed to crouch speed
        } 
        else if (grounded && Input.GetKey(sprintKey))
        {
            state = MovementState.sprinting;
            moveSpeed = sprintSpeed; // Set movement speed to sprint speed
        } 
        else if (grounded)
        {
            state = MovementState.walking;
            moveSpeed = walkSpeed; // Set movement speed to walk speed
        } 
        else
        {
            state = MovementState.air;
        }
    }

    // Moves the player based on input and current movement state
    void MovePlayer()
    {
        if (activeGrapple)
        {
            return;
        }
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        // If the player is on a slope and not exiting the slope, apply slope movement
        if (onslope() && !exitingSlope)
        {
            rb.AddForce(GetSlopeMoveDirection(moveDirection) * moveSpeed * 20f, ForceMode.Force);

            // Applies downward force if the player's vertical velocity is positive (moving upwards)
            if (rb.linearVelocity.y > 0)
            {
                rb.AddForce(UnityEngine.Vector3.down * 80f, ForceMode.Force);
            }
        }
        
        // If the player is grounded, apply movement force based on input
        else if (grounded)
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f, ForceMode.Force);
        } 
        // If the player is not grounded, apply movement force with air multiplier
        else
        {
            rb.AddForce(moveDirection.normalized * moveSpeed * 10f * airMultiplier, ForceMode.Force);
            rb.AddForce(UnityEngine.Vector3.down * 15f, ForceMode.Force);

			rb.linearVelocity *= airDampen;
		}

        // Disables gravity if the player is on a slope
        if(!wallrunning){
            rb.useGravity = !onslope();
        }
    }

    // Controls the speed of the player to ensure it does not exceed the maximum speed
    private void SpeedControl()
    {
        if (activeGrapple)
        {
            return;
        }
        // If the player is on a slope, limit the speed to moveSpeed
        if (onslope() && !exitingSlope)
        {
            if (rb.linearVelocity.magnitude > moveSpeed)
            {
                rb.linearVelocity = rb.linearVelocity.normalized * moveSpeed; // Limiting speed
            }
        }
        else
        {
            UnityEngine.Vector3 flatVel = new UnityEngine.Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

            // If horizontal speed exceeds moveSpeed, limit it
            if (flatVel.magnitude > moveSpeed)
            {
                UnityEngine.Vector3 limitedVel = flatVel.normalized * moveSpeed;
                rb.linearVelocity = new UnityEngine.Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
            }
        }   
    }

    // Initiates the jump by applying an upward force and resetting state for slopes
    private void Jump()
    {
		Debug.Log($"Slide Velocity Before Jump: {rb.linearVelocity}");

		exitingSlope = true; // Indicate that the player is exiting a slope

		rb.linearVelocity = new Vector3(rb.linearVelocity.x, Mathf.Clamp(rb.linearVelocity.y, -5f, 5f), rb.linearVelocity.z);

		if (!wallrunning)
        {
			//rb.linearVelocity = new UnityEngine.Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z); // Reset vertical velocity
			rb.AddForce(transform.up * jumpForce, ForceMode.Impulse); // Apply jump force
		}

		Debug.Log($"Velocity After Jump Force: {rb.linearVelocity}");
	}

    // Resets jump variables after a cooldown period
    private void ResetJump()
    {
        readyToJump = true; // Player is ready to jump again
        exitingSlope = false; // Reset exiting slope state
    }


    bool enableMovementOnNextTouch;
    public void JumpToPosition(UnityEngine.Vector3 targetPosition, float trajectoryHeight)
    {
        activeGrapple = true;
        
        velocityToSet = CalculateJumpVelocity(transform.position, targetPosition, trajectoryHeight);
        Invoke(nameof(SetVelocity), 0.1f);

        Invoke(nameof(ResetRestrictions), 3f);
    }

    private UnityEngine.Vector3 velocityToSet;
    void SetVelocity()
    {
        enableMovementOnNextTouch = true;
        rb.linearVelocity = velocityToSet;


    }

    public void ResetRestrictions()
    {
        activeGrapple = false;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (enableMovementOnNextTouch)
        {
            enableMovementOnNextTouch = false;
            ResetRestrictions();

            GetComponent<Grappling>().stopGrapple();
        }
    }

    // Checks if the player is on a slope by casting a ray downwards
    public bool onslope()
    {
        if (Physics.Raycast(transform.position, UnityEngine.Vector3.down, out slopeHit, height * 0.5f + 0.3f))
        {
            float angle = UnityEngine.Vector3.Angle(UnityEngine.Vector3.up, slopeHit.normal);
            return angle < maxSlopeAngle && angle != 0; // Returns true if angle is within limits
        }

        return false; // Returns false if not on a slope
    }

    // Calculates the movement direction when on a slope
    public UnityEngine.Vector3 GetSlopeMoveDirection(UnityEngine.Vector3 direction)
    {
        return UnityEngine.Vector3.ProjectOnPlane(direction, slopeHit.normal).normalized; // Projects move direction onto slope
    }

    public UnityEngine.Vector3 CalculateJumpVelocity(UnityEngine.Vector3 startPoint, UnityEngine.Vector3 endPoint, float trajectoryHeight)
    {
        float gravity = Physics.gravity.y;
        float displacementY =  endPoint.y - startPoint.y;
        UnityEngine.Vector3 displacementXZ = new UnityEngine.Vector3(endPoint.x - startPoint.x, 0f, endPoint.z - startPoint.z);

        UnityEngine.Vector3 velocityY = UnityEngine.Vector3.up * Mathf.Sqrt(-2 * gravity * trajectoryHeight);
        UnityEngine.Vector3 velocityXZ = displacementXZ / (Mathf.Sqrt(-2 * trajectoryHeight / gravity) 
        + Mathf.Sqrt(2 * (displacementY - trajectoryHeight)/ gravity));

        return velocityXZ + velocityY;
    }

    public void dash(float force)
    {
		ApplyForceOverTime(UnityEngine.Vector3.up, force);
	}

	public void ApplyForceOverTime(UnityEngine.Vector3 direction, float force)
	{
		if (forceCoroutine != null)
		{
			StopCoroutine(forceCoroutine);
		}

		// Start a new force application coroutine
		forceCoroutine = StartCoroutine(ApplyForceCoroutine(orientation.forward, force));
	}

	private IEnumerator ApplyForceCoroutine(UnityEngine.Vector3 direction, float force)
	{
		float elapsedTime = 0f;
		float forcePerSecond = force / 0.15f;

		while (elapsedTime < 0.15f)
		{
			// Apply incremental force
			rb.AddForce(direction * (forcePerSecond * Time.deltaTime), ForceMode.Force);

			elapsedTime += Time.deltaTime;
			yield return null;
		}
	}

    public void slash(float force)
    {
        boxDirection = orientation.forward;

        UnityEngine.Vector3 origin = transform.position;

		ApplyForceOverTime(UnityEngine.Vector3.up, force);

		RaycastHit[] hits = Physics.BoxCastAll(origin, boxSize / 2, boxDirection, UnityEngine.Quaternion.identity, slashDistance, slashLayer);

		foreach (var hit in hits)
        {
			if (hit.collider.CompareTag("Enemy"))
			{
                Debug.Log("ATTACK SLASH");

				eb = hit.collider.GetComponent<EnemyBehavior>();
				if (eb != null)
				{
					eb.TakeDamage(slashDamage);
				}
			}
		}
	}

	private void DebugDrawBoxCast(Vector3 origin, Vector3 size, Vector3 direction, float distance, Color color)
	{
		// Calculate half-extents of the box
		Vector3 halfSize = size / 2;

		// Start and end positions of the box
		Vector3 start = origin;
		Vector3 end = origin + direction.normalized * distance;

		// Draw the start and end boxes
		DrawBox(start, halfSize, Quaternion.identity, color);
		DrawBox(end, halfSize, Quaternion.identity, color);
	}

	// Method to draw a box at a position
	private void DrawBox(Vector3 position, Vector3 halfExtents, Quaternion orientation, Color color)
	{
		// Calculate box corners relative to the position
		Vector3[] corners = new Vector3[8];
		corners[0] = position + orientation * new Vector3(-halfExtents.x, -halfExtents.y, -halfExtents.z);
		corners[1] = position + orientation * new Vector3(halfExtents.x, -halfExtents.y, -halfExtents.z);
		corners[2] = position + orientation * new Vector3(halfExtents.x, -halfExtents.y, halfExtents.z);
		corners[3] = position + orientation * new Vector3(-halfExtents.x, -halfExtents.y, halfExtents.z);
		corners[4] = position + orientation * new Vector3(-halfExtents.x, halfExtents.y, -halfExtents.z);
		corners[5] = position + orientation * new Vector3(halfExtents.x, halfExtents.y, -halfExtents.z);
		corners[6] = position + orientation * new Vector3(halfExtents.x, halfExtents.y, halfExtents.z);
		corners[7] = position + orientation * new Vector3(-halfExtents.x, halfExtents.y, halfExtents.z);

		// Draw box edges
		Debug.DrawLine(corners[0], corners[1], color); // Bottom edges
		Debug.DrawLine(corners[1], corners[2], color);
		Debug.DrawLine(corners[2], corners[3], color);
		Debug.DrawLine(corners[3], corners[0], color);
		Debug.DrawLine(corners[4], corners[5], color); // Top edges
		Debug.DrawLine(corners[5], corners[6], color);
		Debug.DrawLine(corners[6], corners[7], color);
		Debug.DrawLine(corners[7], corners[4], color);
		Debug.DrawLine(corners[0], corners[4], color); // Vertical edges
		Debug.DrawLine(corners[1], corners[5], color);
		Debug.DrawLine(corners[2], corners[6], color);
		Debug.DrawLine(corners[3], corners[7], color);
	}
}