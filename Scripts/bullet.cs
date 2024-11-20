using System.Collections;
using UnityEngine;

public class bullet : MonoBehaviour
{
    public float destroyDelay = 0f; // for animations
    public float damage = 1f;
    public GameObject explosionPrefab;

    private AudioManager audioManager;

    public GameManager gm;

	private void Start()
	{
		audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();

        gm = FindFirstObjectByType<GameManager>();
	}

	void OnCollisionEnter(Collision collision)
    {
        int layerName = LayerMask.NameToLayer("gun");

        // If it hits something that's not the gun layer, process the collision
        if (collision.gameObject.layer != layerName)
        {
            // Check for enemy collision
            if (collision.gameObject.CompareTag("Enemy"))
            {
                EnemyBehavior enemyHealth = collision.gameObject.GetComponent<EnemyBehavior>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(damage);
                }
            }
            // Check for player collision
            else if (collision.gameObject.CompareTag("Player1"))
            {
                gm.takeDamage(damage);
            }

            // Destroy the bullet after the delay
            Destroy(gameObject, destroyDelay);
			Instantiate(explosionPrefab, transform.position, Quaternion.identity);
            audioManager.PlaySFX(audioManager.bulletHit);
		}
    }
}