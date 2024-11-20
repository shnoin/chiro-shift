using System.Numerics;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;

public class WallRunning : MonoBehaviour
{
	[Header("REFERENCES")]
	public Camera camera;

	[Header("Wallrunning")]
	public LayerMask whatIsWall;
	public LayerMask whatIsGround;
	public float wallRunForce;
	public float wallJumpDownForce;
	public float wallJumpSideForce;
	public float wallJumpForwardForce;

	public float wallClimbSpeed;
	public float maxWallRunTime;
	private float wallRunTimer;

	public float cameraTiltRate = 2f;
	public float cameraTiltMax = 70f;
	public float targetRotation;

	public float initialForwardBoost;
	public float initialClimbBoost;

	public bool cameraTilt;
	public bool wallJump;
	public bool cameraAntiTilt;

	[Header("MONITOR")]
	public UnityEngine.Vector3 wallNormal;
	public UnityEngine.Vector3 parallelToWall;

	public UnityEngine.Vector3 forceToApply;

	[Header("Input")]
	private float horizontalInput;
	private float verticalInput;
	public KeyCode jumpKey = KeyCode.Space;

	[Header("Detection")]
	public float wallCheckDistance;
	public float minJumpHeight;
	private RaycastHit leftWallHit;
	private RaycastHit rightWallHit;
	private bool wallLeft;
	private bool wallRight;

	[Header("Exiting")]
	private bool exitingWall;
	public float exitWallTime;
	private float exitWallTimer;

	[Header("Gravity")]
	public bool useGravity;
	public float gravityCounterForce;

	[Header("References")]
	public Transform orientation;
	private Rigidbody rb;
	private Movement pm;

	private Coroutine tiltCoroutine;

	private UnityEngine.Vector3 wallNormalVector;

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
		pm = GetComponent<Movement>();
		wallNormalVector = UnityEngine.Vector3.up;
	}

	private void Update()
	{
		CheckWall();
		StateMachine();

		if (wallLeft)
			targetRotation = -cameraTiltMax;
		else
			targetRotation = cameraTiltMax;

		if (cameraTilt)
		{
			cameraAntiTilt = false;
			wallJump = false;

			UnityEngine.Quaternion targetRotationQ = UnityEngine.Quaternion.Euler(camera.transform.rotation.eulerAngles.x, camera.transform.rotation.eulerAngles.y, targetRotation);
			camera.transform.rotation = UnityEngine.Quaternion.Lerp(camera.transform.rotation, targetRotationQ, cameraTiltRate);
			if (isCloseEnough(camera.transform.rotation, targetRotationQ, 1f))
			{
				cameraTilt = false;
			}
		}

		if ((pm.grounded || (!wallLeft && !wallRight)) && cameraAntiTilt)
		{
			UnityEngine.Quaternion targetRotationQ = UnityEngine.Quaternion.Euler(camera.transform.rotation.eulerAngles.x, camera.transform.rotation.eulerAngles.y, 0f);
			camera.transform.rotation = UnityEngine.Quaternion.Lerp(camera.transform.rotation, targetRotationQ, cameraTiltRate);
			if (isCloseEnough(camera.transform.rotation, targetRotationQ, 1f))
			{
				cameraAntiTilt = false;
				wallJump = false;
			}
		}
	}

	private void FixedUpdate()
	{
		if (pm.wallrunning)
		{
			WallRun();
		}
	}

	private void CheckWall()
	{
		wallRight = Physics.Raycast(transform.position, orientation.right, out rightWallHit, wallCheckDistance, whatIsWall);
		wallLeft = Physics.Raycast(transform.position, -orientation.right, out leftWallHit, wallCheckDistance, whatIsWall);
	}

	private bool AboveGround()
	{
		return !Physics.Raycast(transform.position, UnityEngine.Vector3.down, minJumpHeight, whatIsGround);
	}

	private void StateMachine()
	{
		horizontalInput = Input.GetAxisRaw("horizontal");
		verticalInput = Input.GetAxisRaw("vertical");

		if ((wallLeft || wallRight) && verticalInput > 0 && AboveGround())
		{
			if (Input.GetKeyDown(jumpKey)) WallJump();
		}

		if ((wallLeft || wallRight) && AboveGround() && !exitingWall)
		{
			if (!pm.wallrunning)
			{
				StartWallRun();
			}
			if (wallRunTimer > 0)
				wallRunTimer -= Time.deltaTime;

			if (wallRunTimer <= 0 && pm.wallrunning)
			{
				exitingWall = true;
				exitWallTimer = exitWallTime;
			}
		}
		else if (exitingWall)
		{
			if (pm.wallrunning)
				StopWallRun();

			if (exitWallTimer > 0)
				exitWallTimer -= Time.deltaTime;

			if (exitWallTimer <= 0 && !wallLeft && !wallRight)
				exitingWall = false;

			if (pm.grounded == true)
				exitingWall = false;
		}
		else
		{
			if (pm.wallrunning)
			{
				StopWallRun();
			}
		}
	}

	private void StartWallRun()
	{
		UnityEngine.Vector3 wallForward = UnityEngine.Vector3.Cross(wallNormal, transform.up);
		rb.AddForce(new UnityEngine.Vector3(wallForward.x * initialForwardBoost, initialClimbBoost, wallForward.z * initialForwardBoost));

		// Reset vertical velocity to avoid unintended launch
		rb.linearVelocity = new UnityEngine.Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

		// Set wallrunning state
		cameraTilt = true;
		pm.wallrunning = true;
		wallRunTimer = maxWallRunTime;
	}

	private void WallRun()
	{
		rb.useGravity = useGravity;

		UnityEngine.Vector3 wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;
		UnityEngine.Vector3 wallForward = UnityEngine.Vector3.Cross(wallNormal, transform.up);

		// Ensure the correct wall forward direction
		if ((orientation.forward - wallForward).magnitude > (orientation.forward - -wallForward).magnitude)
		{
			wallForward = -wallForward;
		}

		rb.AddForce(wallForward * wallRunForce * verticalInput, ForceMode.Force);

		// Apply force to maintain wall run
		if (!(wallLeft && horizontalInput > 0) && !(wallRight && horizontalInput < 0))
		{
			rb.AddForce(-wallNormal * 100, ForceMode.Force);
		}

		if (useGravity)
		{
			rb.AddForce(transform.up * gravityCounterForce, ForceMode.Force);
		}
	}

	private void StopWallRun()
	{
		pm.wallrunning = false;
		cameraAntiTilt = true;
		cameraTilt = false;
	}

	private void WallJump()
	{
		wallJump = true;
		cameraTilt = false;

		// enter exiting wall state
		exitWallTimer = exitWallTime;

		wallNormal = wallRight ? rightWallHit.normal : leftWallHit.normal;

		forceToApply = wallNormal * wallJumpSideForce + orientation.forward * wallJumpForwardForce;

		// reset y velocity and add force

		forceToApply = new UnityEngine.Vector3(forceToApply.x * rb.linearVelocity.magnitude, 0f, forceToApply.z * rb.linearVelocity.magnitude);

		exitingWall = true;

		rb.linearVelocity = new UnityEngine.Vector3(rb.linearVelocity.x, wallJumpDownForce, rb.linearVelocity.z);

		rb.AddForce(forceToApply, ForceMode.Impulse);
	}

	private static bool isCloseEnough(UnityEngine.Quaternion pos1, UnityEngine.Quaternion pos2, float tolerance)
	{
		return UnityEngine.Quaternion.Angle(pos1, pos2) <= tolerance;
	}
}

    
