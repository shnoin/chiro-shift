using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class ability2 : MonoBehaviour
{
	[Header("SIG ABILITY 2-1 (3)")]
	public Image abilityImage3;
	public TextMeshProUGUI abilityText3;
	public float abilityCoolDown3 = 20f;

	[Header("SPEC ABILITY 2-2 (4)")]
	public Image abilityImage4;
	public TMPro.TextMeshProUGUI abilityText4;
	public TextMeshProUGUI abilityTextCounter4;
	public float abilityCoolDown4 = 8f;
	public float abilityAmount4 = 2f;

	private bool isAbility3Cooldown = false; // firerate
	private bool isAbility4Cooldown = false; // slash

	private float currentAbility3Cooldown;
	private float currentAbility4Cooldown;

	[Header("REFERENCES")]
	public GameManager gm;

	public GameObject p2;

	public ProjectileGun pistol;
	public ProjectileGun rifle;

	public Movement pm2;

	public Volume volume;

	[Header("SETTINGS")]
	public KeyCode sigKey = KeyCode.E;
	public KeyCode specKey = KeyCode.C;
	public float doubleDuration = 10f;
	public float slashDashForce = 1000f;

	private Vignette vignette;
	private float fireRateTimer;
	public bool doubleOn;

	private Coroutine fireCoroutine;

	private float originalPValue;
	private float originalRValue;

	private AudioManager audioManager;

	private void Start()
	{
		abilityImage3.fillAmount = 1;
		abilityImage4.fillAmount = 1;

		abilityText3.text = "";
		abilityText4.text = "";

		originalPValue = pistol.timeBetweenShooting;
		originalRValue = rifle.timeBetweenShooting;

		audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();
	}

	void Update()
	{
		if (!gm.getCamera())
		{
			if (Input.GetKeyDown(sigKey) && !isAbility3Cooldown)
			{
				SigAbility2();
			}

			if (Input.GetKeyDown(specKey) && !isAbility4Cooldown)
			{
				SpecAbility2();
			}
		}

		AbilityCooldown(ref currentAbility3Cooldown, abilityCoolDown3, ref isAbility3Cooldown, abilityImage3, abilityText3);
		AbilityCooldown(ref currentAbility4Cooldown, abilityCoolDown4, ref isAbility4Cooldown, abilityImage4, abilityText4);

		if (doubleOn)
		{
			abilityImage3.fillAmount = 1;
			abilityText3.text = "X";
		}

		if (gm.getCamera())
		{
			abilityImage3.fillAmount = 1;
			abilityImage4.fillAmount = 1;
		}

		abilityTextCounter4.text = abilityAmount4.ToString();

		if (abilityAmount4 <= 0)
		{
			abilityImage4.fillAmount = 1;
			abilityText4.text = "X";
		}
		else
		{
			if (!gm.getCamera() && !isAbility4Cooldown)
			{
				abilityImage4.fillAmount = 0f;
				abilityText4.text = "";
			}
		}
	}

	public void SigAbility2()
	{
		if (!doubleOn)
		{
			if (fireCoroutine != null)
			{
				StopCoroutine(fireCoroutine);
			}

			fireCoroutine = StartCoroutine(DoubleFirerate(doubleDuration));
		}
		else
		{
			
		}
	}

	public void SpecAbility2()
	{
		if (abilityAmount4 <= 0)
		{
			return;
		}

		audioManager.PlaySFX(audioManager.slash);

		pm2.slash(slashDashForce);

		isAbility4Cooldown = true;
		currentAbility4Cooldown = abilityCoolDown4;
		abilityAmount4 -= 1;
	}

	private IEnumerator DoubleFirerate(float duration)
	{
		doubleOn = true;

		pistol.timeBetweenShooting /= 2;
		rifle.timeBetweenShooting /= 2;

		volume.profile.TryGet(out vignette);

		vignette.color.Override(Color.red);
		vignette.intensity.value = 0.25f;

		yield return new WaitForSeconds(duration);

		resetFirerate();
	}

	public void resetFirerate()
	{
		volume.profile.TryGet(out vignette);

		doubleOn = false;

		pistol.timeBetweenShooting = originalPValue;
		rifle.timeBetweenShooting = originalRValue;

		vignette.color.Override(Color.black);
		vignette.intensity.value = 0f;

		isAbility3Cooldown = true;
		currentAbility3Cooldown = abilityCoolDown3;
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

	public void ToggleFire()
	{
		if (fireCoroutine != null)
		{
			StopCoroutine(fireCoroutine);
			fireCoroutine = null;
		}

		resetFirerate();
	}
}
