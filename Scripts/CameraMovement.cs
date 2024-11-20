using NUnit.Framework.Constraints;
using System.Collections;
using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UIElements;
using UnityEngine.UI;

public class CameraMovement : MonoBehaviour
{
    [Header("REFERENCES")]
    public Volume volume;
    public Transform cameraPos1;
    public Transform cameraPos2;

    public Transform bod1;
    public Transform bod2;

    public ability1 am1;
    public ability2 am2;
    public Movement pm1;
    public Movement pm2;

    public GameObject movementWalls;
    public GameObject shooterWalls;

	public UnityEngine.UI.Image abilityImage1;
	public UnityEngine.UI.Image abilityImage2;
	public UnityEngine.UI.Image abilityImage3;
	public UnityEngine.UI.Image abilityImage4;

	[Header("MAIN SETTINGS")]
    public float cameraSens;
    [SerializeField] private float tolerance = 0.1f;
    [SerializeField] private float distanceTime = 1f;
    [SerializeField] private float zoomDistanceTime = 0.8f;
    [SerializeField] private float fovChange = 35f;
    public float maxChromatic = 1f;
    public float maxDistort = -0.475f;
    public float maxTemp = 30f;
	public float maxVignette = 0.35f;
	public float PPEffectTime= 0.7f; 
	public float fov = 90f;

    [Header("INPUTS")]
    public KeyCode switchKey = KeyCode.F;

    [Header("MONITOR")]
    [SerializeField] private float elapsedTime = 0f;
    [SerializeField] private float speedDistance;
    [SerializeField] private float rotationDistance;
    [SerializeField] private float mouseX;
    [SerializeField] private float mouseY;
    [SerializeField] public float rotationX;
    [SerializeField] public float rotationY;
    [SerializeField] private GameManager GM;

    [HideInInspector] public Transform tempPos;

    private AudioManager audioManager;

	private void Start()
	{
        // Cursor locking & hiding
		UnityEngine.Cursor.lockState = CursorLockMode.Locked;
        UnityEngine.Cursor.visible = false;
        GM = FindFirstObjectByType<GameManager>();

        transform.position = cameraPos1.position;
        GetComponent<Camera>().fieldOfView = fov;

		shooterWalls.SetActive(false);
		movementWalls.SetActive(true);

		audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();
	}

	// Update is called once per frame
	void Update()
    {
        // On switch key -> No camera lock and switch camera bool
		if (Input.GetKeyDown(switchKey))
		{
            audioManager.PlaySFX(audioManager.switchPlayer);

			GM.switchCamera();
			GM.switchLock(false);
			GM.switchLock_(false);

			elapsedTime = 0f;

            StartCoroutine(switchPlayerCameraEffect());
            StartCoroutine(postProcessEffect());

			if (GM.getCamera())
			{
                abilityImage1.fillAmount = 0;
                abilityImage2.fillAmount = 0;
				abilityImage3.fillAmount = 1;
				abilityImage4.fillAmount = 1;

                if (am2.doubleOn)
                    am2.ToggleFire();
			}
			else
			{
				abilityImage2.fillAmount = 1;
				abilityImage1.fillAmount = 1;
				abilityImage3.fillAmount = 0;
				abilityImage4.fillAmount = 0;

                if (am1.grappleOn)
					am1.ToggleGrapple();
			}
		}

        // Set tempPos to the pos of camera going to
		if (GM.getCamera() == true)
		{
			tempPos = cameraPos1;

			setLayerRecursively(bod2.gameObject, "whatIsPlayer");

            if (GM.getLock())
            {
				shooterWalls.SetActive(false);
				movementWalls.SetActive(true);
			}
		}
		else
		{
			tempPos = cameraPos2;

			setLayerRecursively(bod1.gameObject, "whatIsPlayer");

			if (GM.getLock())
			{
				shooterWalls.SetActive(true);
				movementWalls.SetActive(false);
			}
		}

        // If camera is locked and player can control, get input
		if (GM.getLock() == true)
		{
			mouseX = Input.GetAxisRaw("Mouse X") * Time.deltaTime * cameraSens;
			mouseY = Input.GetAxisRaw("Mouse Y") * Time.deltaTime * cameraSens;
		}
        // Else no input
        else
        {
            mouseX = 0f;
            mouseY = 0f;
		}
        
        // If no player control
        if (!GM.getLock())
        {
            // Switch animation
			cameraSwapMovement();
		}

        // Sets speed and rotation speed based on distance * multiplier
		speedDistance = Vector3.Distance(cameraPos1.position, cameraPos2.position);

        rotationDistance = Quaternion.Angle(cameraPos1.rotation, cameraPos2.rotation);

		// If camera is close to moving to position
		if (isCloseEnough(new Vector2(transform.position.x, transform.position.z),
                          new Vector2(tempPos.position.x, tempPos.position.z), tolerance))
        {
            // If first time detected
            if (GM.switchLock(true) == true)
            {
                if (GM.getCamera() == true)
                {
					setLayerRecursively(bod1.gameObject, "InvisibleToCamera");
				}
                else
                {
					setLayerRecursively(bod2.gameObject, "InvisibleToCamera");
				}
                // Set rotationX & rotationY to rotation to have proper camera rotation
                rotationX = tempPos.rotation.eulerAngles.x;
                if (rotationX > 90f)
                {
                    rotationX -= 360;
                }
                rotationY = tempPos.rotation.eulerAngles.y;
                GM.switchLock_(true);
            }
		}
            
        // If player control on
        if (GM.getLock())
        {
            // Lock camera
			transform.position = tempPos.position;
			cameraMouseMovement();
		}    
    }

    private void setLayerRecursively(GameObject obj, string layerName)
    {
        int layer = LayerMask.NameToLayer(layerName);

        obj.layer = layer;
        foreach (Transform child in obj.transform)
        {
            setLayerRecursively(child.gameObject, layerName);
        }
    }

    // Move camera based on rotationX & rotationY
    private void cameraMouseMovement()
    {
        rotationY += mouseX;

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -90f, 90f);

        transform.rotation = Quaternion.Euler(rotationX, rotationY, transform.rotation.eulerAngles.z);
    }

    // Animation to move camera between
    private void cameraSwapMovement()
    {
        if (speedDistance > tolerance && rotationDistance > tolerance)
        {
            elapsedTime += Time.deltaTime;

            // Finds speed needed to travel to travel in distanceTime
            float distanceSpeed = speedDistance / distanceTime;
            float rotationSpeed = rotationDistance / distanceTime;

            // Clamps to 0 - 1 and ease in out
            float t = Mathf.Clamp01(elapsedTime / distanceTime);

            t = easeInOut(t);

            // Performs movements
            transform.position = Vector3.Lerp(transform.position, tempPos.position, t);
            transform.rotation = Quaternion.Lerp(transform.rotation, tempPos.rotation, t);
        }
	}

    // Ease in and out based on quadratic t [0, 1]
    private float easeInOut(float t)
    {
        if (t < 0.5f)
        {
            return 2 * t * t;
        }
        else
        {
            float adjustedT = t - 1;
            return -0.5f * (adjustedT * (adjustedT - 2));
        }
    }

    // Check if two vector2's are close enough with tolerance
    private static bool isCloseEnough(Vector2 pos1, Vector2 pos2, float tolerance)
    {
        return Vector2.Distance(pos1, pos2) <= tolerance;
    }

	IEnumerator switchPlayerCameraEffect()
	{
		// Zoom out
		float initialFOV = GetComponent<Camera>().fieldOfView;
		float targetFOV = initialFOV + fovChange; // adjust for desired zoom out
		float duration = zoomDistanceTime; // adjust for desired zoom speed
		for (float t = 0; t < duration; t += Time.deltaTime)
		{
			GetComponent<Camera>().fieldOfView = Mathf.Lerp(initialFOV, targetFOV, t / duration);
			yield return null;
		}

		// Zoom back in
		for (float t = 0; t < duration; t += Time.deltaTime)
		{
			GetComponent<Camera>().fieldOfView = Mathf.Lerp(targetFOV, initialFOV, t / duration);
			yield return null;
		}
		GetComponent<Camera>().fieldOfView = initialFOV; // ensure it's reset
	}

	IEnumerator postProcessEffect()
	{
		ChromaticAberration chromatic;
		Vignette vignette;
        LensDistortion lensDistortion;
        WhiteBalance whiteBalance;

		volume.profile.TryGet(out chromatic);
		volume.profile.TryGet(out lensDistortion);
		volume.profile.TryGet(out whiteBalance);
		volume.profile.TryGet(out vignette);

		float initialChromatic = 0f;
        float initialDistort = 0f;
        float initialWhiteBalance = 0f;
		float initialVignette = 0f;

		vignette.color.Override(Color.black);

		for (float t = 0; t < PPEffectTime; t += Time.deltaTime)
		{
			chromatic.intensity.value = Mathf.Lerp(initialChromatic, maxChromatic, t / PPEffectTime);
            lensDistortion.intensity.value = Mathf.Lerp(initialDistort, maxDistort, t / PPEffectTime);
            whiteBalance.temperature.value = Mathf.Lerp(initialWhiteBalance, maxTemp, t/ PPEffectTime);
			vignette.intensity.value = Mathf.Lerp(initialVignette, maxVignette, t / PPEffectTime);
			yield return null;
		}

		for (float t = 0; t < PPEffectTime; t += Time.deltaTime)
		{
			chromatic.intensity.value = Mathf.Lerp(maxChromatic, initialChromatic, t / PPEffectTime);
			lensDistortion.intensity.value = Mathf.Lerp(maxDistort, initialDistort, t / PPEffectTime);
			whiteBalance.temperature.value = Mathf.Lerp(maxTemp, initialWhiteBalance, t / PPEffectTime);
			vignette.intensity.value = Mathf.Lerp(maxVignette, initialVignette, t / PPEffectTime);
			yield return null;
		}

		// Return effects to initial values
		chromatic.intensity.value = initialChromatic;
        lensDistortion.intensity.value = initialDistort;
        whiteBalance.temperature.value = initialWhiteBalance;
		vignette.intensity.value = initialVignette;
	}
}
