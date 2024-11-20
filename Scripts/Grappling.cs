using System.Collections;
using UnityEngine;

public class Grappling : MonoBehaviour
{
	[Header("REFERENCES")]
	public LineRenderer lr;
	public Transform gunTip, cam, player, grapple;
	public Movement pm;
	public LayerMask whatIsGrappleable;

	[Header("SETTINGS")]
	public float maxDistance = 100f;
	public float spring;
	public float damper;
	public float massScale;

	private SpringJoint joint;
	private Vector3 grapplePoint;

	private float springR;

	AudioManager audioManager;

	private void Start()
	{
		audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();
	}

	void Update()
	{
		if (pm.isGrappleAvailable)
		{
			if (Input.GetMouseButtonDown(0))
			{
				startGrapple();
			}
			else if (Input.GetMouseButtonUp(0))
			{
				stopGrapple();
			}
		}

		if (!pm.isGrappleAvailable)
		{
			stopGrapple();
		}
	}

	void LateUpdate()
	{
		drawRope();
	}

	void startGrapple()
	{
		audioManager.PlaySFX(audioManager.grapple);

		RaycastHit hit;

		if (Physics.Raycast(cam.position, cam.forward, out hit, maxDistance, whatIsGrappleable))
		{
			grapplePoint = hit.point;

			StartCoroutine(RotateTowardsTarget(grapplePoint));

			joint = player.gameObject.AddComponent<SpringJoint>();
			joint.autoConfigureConnectedAnchor = false;
			joint.connectedAnchor = grapplePoint;

			float distanceFromPoint = Vector3.Distance(player.position, grapplePoint);

			// distance grapple will try to keep from grapple point. 
			joint.maxDistance = distanceFromPoint * 0.8f;
			joint.minDistance = distanceFromPoint * 0.25f;

			if (pm.grounded)
			{
				springR = 100f;
			}
			else
			{
				springR = spring;
			}
			joint.spring = springR;
			joint.damper = damper;
			joint.massScale = massScale;

			lr.positionCount = 2;
			currentGrapplePosition = gunTip.position;
		}
	}

	IEnumerator RotateTowardsTarget(Vector3 target)
	{
		while (true)
		{
			Vector3 direction = target - grapple.position;

			if (direction != Vector3.zero)
			{
				direction.z = 0;

				Quaternion targetRotation = Quaternion.LookRotation(direction);

				grapple.rotation = Quaternion.Lerp(grapple.rotation, targetRotation, 15f * Time.deltaTime);
			}
			else
			{
				break;
			}

			if (Input.GetMouseButtonUp(0))
			{
				grapple.localRotation = Quaternion.identity;
				break;
			}

			yield return null;
		}
	}

	public void stopGrapple()
	{
		lr.positionCount = 0;
		Destroy(joint);
	}

	private Vector3 currentGrapplePosition;

	void drawRope()
	{
		if (!joint) return;

		currentGrapplePosition = Vector3.Lerp(currentGrapplePosition, grapplePoint, Time.deltaTime * 35f);

		lr.SetPosition(0, gunTip.position);
		lr.SetPosition(1, currentGrapplePosition);
	}

	public bool isGrappling()
	{
		return joint != null;
	}

	public Vector3 getGrapplePoint()
	{
		return grapplePoint;
	}
}