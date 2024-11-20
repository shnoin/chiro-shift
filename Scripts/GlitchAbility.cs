using UnityEngine;
using UnityEngine.AI;

public class GlitchAbility : MonoBehaviour
{
	public bool isIdle;

	public NavMeshAgent agent;

	public GameObject particles;

	public Vector3 walkPoint;

	public float walkPointRange;

	public LayerMask whatIsGround;

	public float minInterval = 3f;
	public float maxInterval = 7f;

	private float nextTriggerTime;

	private void Start()
	{
		SetNextTriggerTime();
	}

	private void Update()
	{
		if (!isIdle)
		{
			if (Time.time >= nextTriggerTime)
			{
				Glitch();
				Debug.Log("Glitch");
				SetNextTriggerTime();
			}
		}
	}

	void SetNextTriggerTime()
	{
		nextTriggerTime = Time.time + Random.Range(minInterval, maxInterval);
	}

	private void Glitch()
	{
		SearchWalkPoint();

		Instantiate(particles, transform.position, Quaternion.identity);

		transform.position = walkPoint;
	}
	private void SearchWalkPoint()
	{
		//Calculate random point in range
		float randomZ = Random.Range(-walkPointRange, walkPointRange);
		float randomX = Random.Range(-walkPointRange, walkPointRange);

		walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);
	}
}
