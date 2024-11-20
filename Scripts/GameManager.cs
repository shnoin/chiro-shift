using System.Collections;
using System.Transactions;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [Header("REFERENCES")]
    public GameObject completeLevelUI;

    public Transform l1SpawnPoint;
    public Transform l2SpawnPoint;
    public GameObject defaultEnemy;
    public GameObject tankEnemy;
    public GameObject glitchEnmey;

    public ability1 am1;
    public ability2 am2;

    public GameObject l1Enemies;
    public GameObject l1Wall;

    public GameObject p1;
    public GameObject p2;

    [Header("SPAWNING")]
    public float minInterval1 = 1f;
    public float maxInterval1 = 3.5f;

	public float minInterval2 = 25f;
	public float maxInterval2 = 35f;

	public float minInterval3 = 1f;
	public float maxInterval3 = 3.5f;

    public Vector3 checkpointPos;

	[Header("CAMERA SWITCHING")]
	public bool currentPlayerPos1 = true;
    public bool currentPlayerPosLock = true;

    [Header("PLAYER MONITOR")]
    public int current1GunEquiped;
    public int current2GunEquiped;

	public float maxHealth = 12f;
	public float health = 12f;

	public const int MAGNUM = 1;
    public const int M4 = 2;
    public const int KATANA = 3;

    public bool playerDoor;
    public bool finalDoor;

    public bool l1;
    public bool l2;
    public bool l3;

    [HideInInspector] public bool currentPlayerPosLock_ = true;

    public bool ended;

    private AudioManager audioManager;

    private float nextTriggerTime1;
    private float nextTriggerTime2;
    private float nextTriggerTime3;

	public void Start()
	{
        ended = false;

		audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();
	}

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.I))
        {
            SceneManager.LoadScene("Level");
        }

        if (Time.time >= nextTriggerTime1 && l1)
        {
            SpawnEnemy(defaultEnemy, l1SpawnPoint);
            SetNextTriggerTime(ref nextTriggerTime1, minInterval1, maxInterval1);
        }

        if (Time.time >= nextTriggerTime2 && l2)
        {
            SpawnEnemy(tankEnemy, l2SpawnPoint);
			SetNextTriggerTime(ref nextTriggerTime2, minInterval2, maxInterval2);
		}

        SetCheckpoint();
    }

    public void SpawnEnemy(GameObject enemy, Transform location)
    {
        Instantiate(enemy, location.position, Quaternion.identity);
    }

    public void ResetStats()
    {
        health = maxHealth;

        am1.abilityAmount2 = 2;
        am2.abilityAmount4 = 2;
    }

	void SetNextTriggerTime(ref float nextTriggerTime, float minInterval, float maxInterval)
	{
		nextTriggerTime = Time.time + Random.Range(minInterval, maxInterval);
	}

	public void ResetTutLevel()
    {
		SceneManager.LoadScene("Tutorial'");
	}

	public void ResetLevLevel()
	{
		SceneManager.LoadScene("Level");
	}

    public void SetCheckpoint()
    {
        if (l2)
        {
            checkpointPos = l1SpawnPoint.position;
        }
        else if (l3)
        {
            checkpointPos = l2SpawnPoint.position;
        }
    }

    public Vector3 GetCheckPoint()
    {
        return checkpointPos;
    }

	public void switchCamera()
    {
        currentPlayerPos1 = !currentPlayerPos1;
    }

    public bool getCamera()
    {
        return currentPlayerPos1;
    }

    public bool switchLock(bool value)
    {
        currentPlayerPosLock = value;
		if (currentPlayerPosLock != currentPlayerPosLock_)
        {
            return true;
        }
        return false;
    }

    public void switchLock_(bool value)
    {
        currentPlayerPosLock_ = value;
    }

    public bool getLock()
    {
        return currentPlayerPosLock;
    }

    public void setGun(int gun)
    {
        if (currentPlayerPos1)
        {
            current1GunEquiped = gun;
        }
        else
        {
            current2GunEquiped = gun;
        }
    }
    public int getGun()
    {
        if (currentPlayerPos1)
        {
            return current1GunEquiped;
        }
        else
        {
            return current2GunEquiped;
        }
    }

    public IEnumerator LevelEnd()
    {
		yield return new WaitForSeconds(0.75f);

        completeLevelUI.SetActive(true);

        ended = true;

        audioManager.PlaySFX(audioManager.specClick);

		yield return new WaitForSeconds(0.5f);

		SceneManager.LoadScene("Level");

        completeLevelUI.SetActive(false);

        l1 = true;
    }

	public void takeDamage(float damage)
	{
		health -= damage;

		if (health <= 0)
		{
			audioManager.PlaySFX(audioManager.death);

			Vector3 respawnPos = GetCheckPoint();

			p1.transform.position = respawnPos;
			p2.transform.position = respawnPos;

			ResetStats();
		}

		audioManager.PlaySFX(audioManager.ouch);
	}

    public bool OpenDoor()
    {
        return playerDoor;
    }

    public bool FinalOpenDoor()
    {
        return finalDoor;
    }
}
