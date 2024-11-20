
using UnityEngine;
using UnityEngine.AI;

public class EnemyBehavior : MonoBehaviour
{
	public bool isIdle;

	public NavMeshAgent agent;

	public Animator animator;

	public GameManager gm;

	public GameObject explosionPrefab;

	public GameObject player1;
	public GameObject player2;

	public Transform bulletNozzle;

	public LayerMask whatIsGround, whatIsPlayer;

	public float health;

	//Patroling
	public Vector3 walkPoint;
	bool walkPointSet;
	public float walkPointRange;

	//Attacking
	public float timeBetweenAttacks;
	bool alreadyAttacked;
	public GameObject projectile;

	//States
	public float sightRange, attackRange;
	public bool playerInSightRange, playerInAttackRange;

	private GameObject player;

	private AudioManager audioManager;

	private void Awake()
	{
		agent = GetComponent<NavMeshAgent>();

		audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();

		player1 = GameObject.Find("Movement");
		player2 = GameObject.Find("Movement");
	}

	private void Update()
	{
		if (!isIdle)
		{
			float distanceTo1 = Mathf.Abs(transform.position.x - player1.transform.position.x);
			float distanceTo2 = Mathf.Abs(transform.position.x - player2.transform.position.x);


			if (distanceTo1 > distanceTo2)
			{
				player = player2;
			}
			else
			{
				player = player1;
			}

			//Check for sight and attack range
			Collider[] hit;
			hit = Physics.OverlapSphere(transform.position, sightRange, whatIsPlayer);

			foreach (Collider c in hit)
			{
				if (c.transform.root.CompareTag("Player1"))
				{
					playerInSightRange = true;
					break;
				}
				else
				{
					playerInSightRange = false;
				}
			}

			hit = Physics.OverlapSphere(transform.position, attackRange, whatIsPlayer);

			foreach (Collider c in hit)
			{
				if (c.transform.root.CompareTag("Player1"))
				{
					playerInAttackRange = true;
					break;
				}
				else
				{
					playerInAttackRange = false;
				}
			}

			if (!playerInSightRange && !playerInAttackRange)
			{
				Patroling();
				animator.SetFloat("Blend", 1f);
			}
			if (playerInSightRange && !playerInAttackRange)
			{
				ChasePlayer();
				animator.SetFloat("Blend", 1f);
			}
			if (playerInAttackRange && playerInSightRange)
			{
				AttackPlayer();
				animator.SetFloat("Blend", 0f);
			}
		}
	}

	private void Patroling()
	{
		if (!walkPointSet) SearchWalkPoint();

		if (walkPointSet)
			agent.SetDestination(walkPoint);

		Vector3 distanceToWalkPoint = transform.position - walkPoint;

		//Walkpoint reached
		if (distanceToWalkPoint.magnitude < 1f)
			walkPointSet = false;
	}
	private void SearchWalkPoint()
	{
		//Calculate random point in range
		float randomZ = Random.Range(-walkPointRange, walkPointRange);
		float randomX = Random.Range(-walkPointRange, walkPointRange);

		walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

		if (Physics.Raycast(walkPoint, -transform.up, 2f, whatIsGround))
			walkPointSet = true;
	}

	private void ChasePlayer()
	{
		agent.SetDestination(player.transform.position);
	}

	private void AttackPlayer()
	{
		//Make sure enemy doesn't move
		agent.SetDestination(transform.position);

		transform.LookAt(player.transform);

		if (!alreadyAttacked)
		{
			///Attack code here
			Rigidbody rb = Instantiate(projectile, bulletNozzle.position, Quaternion.identity).GetComponent<Rigidbody>();
			rb.AddForce(transform.forward * 10f, ForceMode.Impulse);
			///End of attack code

			alreadyAttacked = true;
			Invoke(nameof(ResetAttack), timeBetweenAttacks);
		}
	}
	private void ResetAttack()
	{
		alreadyAttacked = false;
	}

	public void TakeDamage(float damage)
	{
		health -= damage;

		if (health <= 0) DestroyEnemy();
	}
	private void DestroyEnemy()
	{
		Instantiate(explosionPrefab, transform.position, Quaternion.identity);
		audioManager.PlaySFX(audioManager.enemyDeath);
		Destroy(gameObject);
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, attackRange);
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(transform.position, sightRange);
	}
}
