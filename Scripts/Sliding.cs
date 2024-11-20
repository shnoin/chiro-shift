using UnityEngine;

public class Sliding : MonoBehaviour
{
    
    
    [Header("References")]
    public Transform orientation;
    public Transform playerObj;
    private Rigidbody rb;
    private Movement pm;
    private GameManager manager;

    public bool player1;

    [Header("Sliding")]
    public float slideForce;
    public float slideDecay;
    public float slideTimer;

    public float slideYScale;
    private float startYScale;

    public Vector3 slideVelocity;

    [Header("Input")]
    public KeyCode slideKey = KeyCode.C;
    private float horizontalInput;
    private float verticalInput;
    public bool isSliding;

    private void Start(){
        rb = GetComponent<Rigidbody>();
        pm = GetComponent<Movement>();

        manager = FindFirstObjectByType<GameManager>();

		startYScale = playerObj.localScale.y;
    }

    private void Update(){
		if (player1 == manager.getCamera())
        {
			horizontalInput = Input.GetAxisRaw("horizontal");
			verticalInput = Input.GetAxisRaw("vertical");

			if (Input.GetKeyDown(slideKey))
			{
				startSlide();
			}
			if (Input.GetKeyUp(slideKey))
			{
				stopSlide();
			}
		}
    }

    private void FixedUpdate(){
        if (isSliding && player1 == manager.getCamera())
        {
			slidingMovement();
        }
    }

    private void startSlide(){
        isSliding = true;
        playerObj.localScale = new Vector3(playerObj.localScale.x, slideYScale, playerObj.localScale.z);
        rb.AddForce(Vector3.down * 5f, ForceMode.Impulse);

		slideTimer = 0;
		slideVelocity = rb.linearVelocity;

        if (pm.grounded && rb.linearVelocity.magnitude > 0.5f)
        {
            rb.AddForce(orientation.transform.forward * slideForce);
        }
	}

	private void slidingMovement()
	{
		Vector3 inputDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

		slideTimer += Time.deltaTime;

		if (pm.onslope())
		{
			rb.AddForce(Vector3.down * 300f);
		}
		else
		{
			slideVelocity *= Mathf.Pow(slideDecay, slideTimer);
		}

		rb.linearVelocity = new Vector3(slideVelocity.x, rb.linearVelocity.y, slideVelocity.z);
	}

	public void stopSlide(){
        isSliding = false;
        playerObj.localScale = new Vector3(playerObj.localScale.x, startYScale, playerObj.localScale.z);

        slideVelocity = Vector3.zero;
    }
}
