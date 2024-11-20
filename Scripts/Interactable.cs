using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class Interactable : MonoBehaviour
{
    [Header("DEFAULT")]
	Outline outline;
	public string message;
	public UnityEvent onInteraction;
	public GameManager gm;
	public AudioManager audioManager;

	[Header("GUN")]
    public Transform gunPos1;
	public int gun;

	[Header("TUTORIAL")]
    public GameObject glass;
    public GameObject bridge;

    [Header("LEVEL 1")]
    public Transform l1Special;

    public Animator l1Animator;

    [Header("LEVEL 2")]
    public GameObject outsideBridge;
    public Animator l2Animator;

    public Animator l2FinalAnimator;

	// Start is called once before the first execution of Update after the MonoBehaviour is created
	void Start()
    {
        outline = GetComponent<Outline>();
        DisableOutLine();

        gm = FindFirstObjectByType<GameManager>();

		audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();
	}

    public void Interact()
    {
        onInteraction.Invoke();
    }

    public void DisableOutLine()
    {
        outline.enabled = false;
    }

    public void EnableOutLine()
    {
        outline.enabled = true;
    }

    public void pickupGun()
    {
        transform.position = gunPos1.position;
        transform.rotation = gunPos1.rotation;

        gm.setGun(gun);
    }

    public void tutorialButton()
    {
        glass.SetActive(false);
        bridge.SetActive(true);

        audioManager.PlaySFX(audioManager.regClick);
    }

    public void Level1MovementPlatform()
    {
        foreach (Transform child in l1Special)
        {
            child.gameObject.SetActive(true);
        }

        l1Special.gameObject.SetActive(true);

		audioManager.PlaySFX(audioManager.regClick);
	}

    public void Level1Complete()
    {
        l1Animator.SetBool("PlayAnimation", true);

        audioManager.PlaySFX(audioManager.specClick);

        gm.l1 = false;
        gm.l2 = true;

        gm.ResetStats();
        gm.SetCheckpoint();
    }

    public void UnderneathButton()
    {
        audioManager.PlaySFX(audioManager.regClick);

        outsideBridge.SetActive(true);
        gm.playerDoor = true;
    }

    public void Level2Complete()
    {
        l2FinalAnimator.SetBool("StartAnimation", true);

		audioManager.PlaySFX(audioManager.specClick);

        gm.l2 = false;
        gm.l3 = true;

		gm.ResetStats();
	}
}



