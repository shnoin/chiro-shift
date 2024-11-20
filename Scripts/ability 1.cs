using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class ability1 : MonoBehaviour
{
	[Header("SIG ABILITY 1-1 (1)")]
	public Image abilityImage1;
	public TextMeshProUGUI abilityText1;
	public float abilityCoolDown1 = 5f;

	[Header("SPEC ABILITY 1-2 (2)")]
	public Image abilityImage2;
	public TextMeshProUGUI abilityText2;
	public TextMeshProUGUI abilityTextCounter2;
	public float abilityCoolDown2 = 3f;
	public float abilityAmount2 = 2f;

	private bool isAbility1Cooldown = false; // grapple
	private bool isAbility2Cooldown = false; // dash

	private float currentAbility1Cooldown;
	private float currentAbility2Cooldown;

	[Header("REFERENCES")]
	public GameManager gm;

	public GameObject p1;
	public GameObject grapple;

	public Movement pm1;

	public Volume volume;

	[Header("SETTINGS")]
	public KeyCode sigKey = KeyCode.E;
	public KeyCode specKey = KeyCode.C;
	public float dashForce = 3f;
	public float grappleDuration = 10f;

	private Vignette vignette;
	public bool grappleOn;

	private Coroutine grappleCoroutine;

	private AudioManager audioManager;

	private void Start()
	{
		grapple.SetActive(false);

		abilityImage1.fillAmount = 0;
		abilityImage2.fillAmount = 0;

		abilityText1.text = "";
		abilityText2.text = "";

		audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();
	}

	void Update()
	{
		if (gm.getCamera())
		{
			if (Input.GetKeyDown(sigKey) && !isAbility1Cooldown)
			{
				SigAbility1();
			}

			if (Input.GetKeyDown(specKey) && !isAbility2Cooldown)
			{
				SpecAbility1();
			}
		}

		AbilityCooldown(ref currentAbility1Cooldown, abilityCoolDown1, ref isAbility1Cooldown, abilityImage1, abilityText1);
		AbilityCooldown(ref currentAbility2Cooldown, abilityCoolDown2, ref isAbility2Cooldown, abilityImage2, abilityText2);

		if (grappleOn)
		{
			abilityImage1.fillAmount = 1;
			abilityText1.text = "X";
		}

		abilityTextCounter2.text = abilityAmount2.ToString();

		if (abilityAmount2 <= 0)
		{
			abilityImage2.fillAmount = 1;
			abilityText2.text = "X";
		}
		else
		{
			if (gm.getCamera() && !isAbility2Cooldown)
			{
				abilityImage2.fillAmount = 0f;
				abilityText2.text = "";
			}
		}
	}

	public void SigAbility1()
	{
		if (!grappleOn)
		{
			if (grappleCoroutine != null)
			{
				StopCoroutine(grappleCoroutine);
			}

			grappleCoroutine = StartCoroutine(Grapple(grappleDuration));
		}
		else
		{
			ToggleGrapple();
		}
	}

	public void SpecAbility1()
	{
		if (abilityAmount2 <= 0)
		{
			return;
		}

		audioManager.PlaySFX(audioManager.dash);
		pm1.dash(dashForce);

		isAbility2Cooldown = true;
		currentAbility2Cooldown = abilityCoolDown2;
		abilityAmount2 -= 1;
	}

	private IEnumerator Grapple(float duration)
	{
		grappleOn = true;

		pm1.isGrappleAvailable = true;

		volume.profile.TryGet(out vignette);

		grapple.SetActive(true);
		vignette.color.Override(Color.blue);
		vignette.intensity.value = 0.25f;

		yield return new WaitForSeconds(duration);

		EndGrapple();
	}

	public void EndGrapple()
	{
		volume.profile.TryGet(out vignette);

		grappleOn = false;

		pm1.isGrappleAvailable = false;

		grapple.SetActive(false);

		vignette.color.Override(Color.black);
		vignette.intensity.value = 0f;

		isAbility1Cooldown = true;
		currentAbility1Cooldown = abilityCoolDown1;
	}

	private void AbilityCooldown(ref float currentCooldown, float maxCooldown, ref bool isCooldown, Image skillImage, TextMeshProUGUI skillText)
	{
		if (isCooldown)
		{
			currentCooldown -= Time.deltaTime;

			if (currentCooldown <= 0)
			{
				isCooldown = false;
				currentCooldown = 0f;

				if (skillImage != null)
				{
					skillImage.fillAmount = 0f;
				}

				if (skillText != null)
				{
					skillText.text = "";
				}
			}
			else
			{
				if (skillImage != null)
				{
					skillImage.fillAmount = currentCooldown / maxCooldown;
				}

				if (skillText != null)
				{
					skillText.text = Mathf.Ceil(currentCooldown).ToString();
				}
			}
		}
	}

	public void ToggleGrapple()
	{
		if (grappleCoroutine != null)
		{
			StopCoroutine(grappleCoroutine);
			grappleCoroutine = null;
		}

		EndGrapple();
	}
}
