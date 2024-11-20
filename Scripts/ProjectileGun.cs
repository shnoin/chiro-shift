
using TMPro;
using UnityEngine;

public class ProjectileGun : MonoBehaviour
{
    public GameObject bullet;
    public Camera cam;
    public bool equipped;

    public float shootForce;


    [Header("Gun stats")]
    public float timeBetweenShooting;
    public float spread; 
    public float timeBetweenShots;
    public int bulletsPerTap;
    public bool isAuto;
    int bulletsLeft, bulletsShot;
    bool shooting, readyToShoot, reloading; 

    [Header("References")]

    public Camera fpsCam;
    public Transform attackPoint;
    public GameManager gameManager;
    public GameManager gm;
    public float gun;

    [Header("Graphics")]
    public GameObject muzzleFlash;

    private AudioManager audioManager;

    void Awake()
    {
        readyToShoot = true;
        audioManager = GameObject.FindGameObjectWithTag("AudioManager").GetComponent<AudioManager>();
	}

    void Update()
    {
        MyInput();

        if (gun == gm.getGun())
        {
            equipped = true;
        }
        else
        {
            equipped = false;
        }
    }

    void MyInput()
    {
        if (!equipped)
        {
            return;
        }

        if(isAuto)
            shooting = Input.GetKey(KeyCode.Mouse0);
        
        else
            shooting = Input.GetKeyDown(KeyCode.Mouse0);
        
        if (readyToShoot && shooting && equipped)
        {
            bulletsShot = 0;
            Shoot();
        }
    }

    void Shoot()
    {
        audioManager.PlaySFX(audioManager.shoot);

        readyToShoot = false;

        Ray ray = fpsCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        Vector3 targetPoint;
        if (Physics.Raycast(ray, out hit))
        {
            targetPoint = hit.point;
        }   
        else
        {
            targetPoint = ray.GetPoint(75);
        }

        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);
        Vector3 direction = (targetPoint - attackPoint.position) + new Vector3(x, y, 0);

        GameObject currentBullet = Instantiate(bullet, attackPoint.position, cam.transform.rotation);
        currentBullet.transform.forward = direction.normalized;

        currentBullet.GetComponent<Rigidbody>().AddForce(direction.normalized * shootForce, ForceMode.Impulse);

        if (muzzleFlash != null)
        {
            Instantiate(muzzleFlash, attackPoint.position, Quaternion.identity);
        }

        bulletsLeft--;
        bulletsShot++;

        Invoke(nameof(ResetShot), timeBetweenShooting);

        if (bulletsShot < bulletsPerTap && bulletsLeft > 0)
        {
            Invoke(nameof(Shoot), timeBetweenShooting);
        }
    }

    void ResetShot()
    {
        readyToShoot = true;
    }
}
