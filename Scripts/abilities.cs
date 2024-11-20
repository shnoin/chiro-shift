using System.Collections;
using TMPro;
using Unity.Jobs;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class abilities : MonoBehaviour
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

	[Header("SIG ABILITY 2-1 (3)")]
	public Image abilityImage3;
	public TextMeshProUGUI abilityText3;
	public float abilityCoolDown3 = 20f;

	[Header("SPEC ABILITY 2-2 (4)")]
	public Image abilityImage4;
	public TextMeshProUGUI abilityText4;
	public TextMeshProUGUI abilityTextCounter4;
	public float abilityCoolDown4 = 8f;
    public float abilityAmount4 = 2f;

    private bool isAbility1Cooldown = false; // grapple
    private bool isAbility2Cooldown = false; // dash
    private bool isAbility3Cooldown = false; // firerate
    private bool isAbility4Cooldown = false; // slash

    private float currentAbility1Cooldown;
    private float currentAbility2Cooldown;
    private float currentAbility3Cooldown;
    private float currentAbility4Cooldown;

	[Header("REFERENCES")]
    public GameManager gm;

    public bool player1;

    public GameObject p1;
    public GameObject p2;
    public GameObject grapple;

    public ProjectileGun pistol;
    public ProjectileGun rifle;

    public Movement pm1;
    public Movement pm2;

    public Volume volume;

    [Header("SETTINGS")]
    public KeyCode sigKey = KeyCode.E;
    public KeyCode specKey = KeyCode.C;
    public float dashForce = 3f;
    public float doubleDuration = 10f;
    public float grappleDuration = 10f;

	private Vignette vignette;
    private float fireRateTimer;
    public bool grappleOn;
    public bool doubleOn;

	private Coroutine grappleCoroutine;
    private Coroutine fireCoroutine;

    private float originalPValue;
    private float originalRValue;

	private void Start()
	{
        grapple.SetActive(false);

        abilityImage1.fillAmount = 0;
        abilityImage2.fillAmount = 0;
        abilityImage3.fillAmount = 1;
        abilityImage4.fillAmount = 1;

        abilityText1.text = "";
        abilityText2.text = "";
        abilityText3.text = "";
        abilityText4.text = "";

		originalPValue = pistol.timeBetweenShooting;
		originalRValue = rifle.timeBetweenShooting;
	}

	void Update()
    {
        if (player1 && gm.getCamera())
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

        if (!player1 && !gm.getCamera())
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

        AbilityCooldown(ref currentAbility1Cooldown, abilityCoolDown1, ref isAbility1Cooldown, abilityImage1, abilityText1);
        AbilityCooldown(ref currentAbility2Cooldown, abilityCoolDown2, ref isAbility2Cooldown, abilityImage2, abilityText2);
        AbilityCooldown(ref currentAbility3Cooldown, abilityCoolDown3, ref isAbility3Cooldown, abilityImage3, abilityText3);
        AbilityCooldown(ref currentAbility4Cooldown, abilityCoolDown4, ref isAbility4Cooldown, abilityImage4, abilityText4);

        if (grappleOn)
        {
            abilityImage1.fillAmount = 1;
            abilityText1.text = "X";
        }

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
        else
        {
			abilityImage2.fillAmount = 1;
			abilityImage1.fillAmount = 1;
		}

        abilityTextCounter2.text = abilityAmount2.ToString();
        abilityTextCounter4.text = abilityAmount4.ToString();

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

        if (abilityAmount4 <= 0)
        {
            abilityImage4.fillAmount = 1;
            abilityText4.text = "X";
        }
		else
		{
            if (!gm.getCamera() && !isAbility4Cooldown)
            {
                Debug.Log("Force");
				abilityImage4.fillAmount = 0f;
				abilityText4.text = "";
			}
		}
        Debug.Log("Camera: " + gm.getCamera());
        Debug.Log("Cooldown: " + isAbility4Cooldown);
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
            if (grappleCoroutine != null)
            {
                StopCoroutine(grappleCoroutine);
                grappleCoroutine = null;
            }
            
            EndGrapple();
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
            if (fireCoroutine != null)
            {
                StopCoroutine(fireCoroutine);
                fireCoroutine = null;
            }

            resetFirerate();
        }
	}

    public void SpecAbility1()
    {
        if (abilityAmount2 <= 0)
        {
            return;
        }

		pm1.dash(dashForce);

		isAbility2Cooldown = true;
		currentAbility2Cooldown = abilityCoolDown2;
        abilityAmount2 -= 1;
	}

    public void SpecAbility2()
    {
        if (abilityAmount4 <= 0)
        {
            return;
        }

        Debug.Log("called");

		isAbility4Cooldown = true;
		currentAbility4Cooldown = abilityCoolDown4;
		abilityAmount4 -= 1;
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
}
