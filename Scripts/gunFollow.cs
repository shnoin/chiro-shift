using UnityEngine;

public class gunFollow : MonoBehaviour
{
    [Header("REFERENCE")]
    public GameManager gameManager;
    public Transform gunPos;
    public Rigidbody rb;
    private ProjectileGun pg;
    public Camera cam;

    [Header("SETTINGS")]
    public int gun;
    public KeyCode dropGun = KeyCode.Q;
    public float throwForce = 3f;

    [Header("MONITOR")]
    public bool equipped;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
		gameManager = FindFirstObjectByType<GameManager>();
        rb = GetComponent<Rigidbody>();

        pg = GetComponent<ProjectileGun>();
	}

    // Update is called once per frame
    void Update()
    {
        if (gameManager.getGun() == gun)
        {
            transform.SetParent(gunPos);

            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;

            rb.useGravity = false;
            rb.angularVelocity = Vector3.zero;
            equipped = true;

            if (pg != null)
            {
                pg.equipped = true;
            }
        }
        else
        {
			transform.SetParent(null);

            rb.useGravity = true;
            rb.angularVelocity = Vector3.zero;

			rb.AddForce(Vector3.down * 0.1f, ForceMode.Impulse);

			equipped = false;
        }

        if (Input.GetKeyDown(dropGun) && gameManager.getGun() == gun)
        {
			transform.SetParent(null);

			rb.useGravity = true;
            rb.isKinematic = false;
			rb.angularVelocity = Vector3.zero;

			equipped = false;

			gameManager.setGun(0);

            rb.AddForce(cam.transform.forward * throwForce, ForceMode.Impulse);
		}
    }
}
