using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

/*
    Este script gestiona el disparo de un arma de fuego, incluyendo el manejo del comportamiento de disparo, recarga, munici�n, da�o y efectos visuales.
*/



public class GunController : MonoBehaviour
{

    public bool activateRecoil, activateSpread, activateADS;


    int currentBurst = 0;

    bool weaponActive;
    [Header("GunData")]
    AudioSource audioSource;
    public string gunName;
    public int id, ammoCost, pellets, damagePellet;
    public float damage;
    public Animator gunAnimator;
    public LayerMask enemyMask;

    public float recoilSmoothness, recoilXmin, recoilXmax, recoilYmin, recoilYmax;
    private Quaternion originalCameraRotation;
    [Header("Firing")]
    public ParticleSystem muzzleFX;
    private float firing_coolDown, t_fire;
    [SerializeField] private bool headShot, isFiring;
    public float rateOfFire, headshotMultiplier, range;
    enum FiringMode { semi, full, buckshot }
    [SerializeField] FiringMode firingMode;
    [SerializeField] AudioClip[] soundEffects;
    [SerializeField] AudioClip[] footstepSoundEffects;
    [Header("Spread")]
    public Camera cam;
    public float spreadMinX, spreadMaxX, spreadMinY, spreadMaxY;
    float spreadX, spreadY;
    Vector3 spread;

    [Header("DisplayAmmunition")]
    public int ammo;
    public int reserveAmmo;
    [Header("InternalAmmunition")]
    public int ammoCapacity;
    public int extraMags, initialMags;
    [Header("Reload")]
    public bool isReloading;
    public float reloadTime, emptyReloadTime;
    private float defaultSpeed = 1, speedMultiplier = 1.5f;

    public Vector3 originalPos;
    public Vector3 adsPos;
    public float adsTime;
    public bool isAiming;

    [Header("WeaponSway")]
    #region WeaponSway
    public float swayMultiplier;
    public float swaySmoothness;
    #endregion

    void Start()
    {
        originalPos = GameObject.Find("GunPos").transform.localPosition;
        cam = GameObject.FindGameObjectWithTag("PlayerCamera").GetComponent<Camera>();
        originalCameraRotation = Quaternion.Euler(0, 0, 0);
        audioSource = transform.root.GetComponent<AudioSource>();
        ammo = ammoCapacity;
        reserveAmmo = ammoCapacity * initialMags; t_fire = firing_coolDown;
        cam = GameObject.Find("fpsCam").GetComponent<Camera>();

        if (gameObject.transform.root.GetComponent<PlayerController>().speedcola_Active)
        {
            IncreaseReloadSpeed();
        }
        defaultSpeed = gunAnimator.GetFloat("speedMultiplier");
    }

    public void OnEnable()
    {
        if (gameObject.transform.root.GetComponent<PlayerController>().speedcola_Active)
        {
            IncreaseReloadSpeed();
        }
        weaponActive = true;
        InputManager.Instance._inputActions.Player.Reload.started += ctx => Reload();
        InputManager.Instance._inputActions.Player.Fire.started += ctx => StartShot();
        InputManager.Instance._inputActions.Player.Fire.canceled += ctx => EndShot();
        InputManager.Instance._inputActions.Player.AimDownSight.started += ctx => StartADS();
        InputManager.Instance._inputActions.Player.AimDownSight.canceled += ctx => EndADS();
    }
    public void OnDisable()
    {
        if (gameObject.transform.root.GetComponent<PlayerController>().speedcola_Active)
        {
            DefaultReloadSpeed();
        }
        weaponActive = false;
        InputManager.Instance._inputActions.Player.Reload.started -= ctx => Reload();
        InputManager.Instance._inputActions.Player.Fire.started -= ctx => StartShot();
        InputManager.Instance._inputActions.Player.Fire.canceled -= ctx => EndShot();
        InputManager.Instance._inputActions.Player.AimDownSight.started -= ctx => StartADS();
        InputManager.Instance._inputActions.Player.AimDownSight.canceled -= ctx => EndADS();
    }

    void Update()
    {

        // Calculo de la cadencia de disparo
        firing_coolDown = 60 / rateOfFire;
        // Si el temporizador de disparo es menor que el enfriamiento le suma Time.deltaTime
        if (t_fire < firing_coolDown)
        {
            t_fire += Time.deltaTime;
        }

        // Inicia la recarga si la munici�n est� agotada
        if (ammo == 0 && reserveAmmo > 0 && !GameManager.Instance.isPaused && !isReloading && t_fire >= firing_coolDown)
        {
            Reload();
            Debug.Log(ammo);
        }

        // Inicia la recarga si se presiona la tecla de recarga y hay munici�n disponible
        if (isAiming && !isReloading && activateADS)
        {
            AimDownSight();
        }
        else
        {
            HipFire();
        }
        // Comprueba el modo de disparo y la disponibilidad de munici�n y si se est� recargando antes de disparar
        if (firingMode.Equals(FiringMode.semi) && weaponActive && isFiring)
        {
            Fire();

        }
        else if (firingMode.Equals(FiringMode.full))
        {
            if (isFiring == true && ammo > 0 && t_fire >= firing_coolDown && !isReloading && GameManager.Instance.isPaused == false)
            {
                Fire();
            }
        }
        else if (firingMode.Equals(FiringMode.buckshot) && weaponActive && isFiring)
        {
            BuckShot();
        }
        if (activateRecoil)
        {
            RecoilEnd();

        }
        WeaponSway();
    }

    // Realiza un disparo


    private void WeaponSway()
    {
        Vector2 sway = InputManager.Instance._inputActions.Player.MouseLook.ReadValue<Vector2>() + InputManager.Instance._inputActions.Player.ControllerLook.ReadValue<Vector2>() * 10;
        Quaternion swayX = Quaternion.AngleAxis(sway.y, Vector3.right);
        Quaternion swayY = Quaternion.AngleAxis(-sway.x, Vector3.up);
        Quaternion targetRotation = swayX * swayY;
        transform.localRotation = Quaternion.Slerp(transform.localRotation, targetRotation, Time.deltaTime * swaySmoothness);
    }
    private void ApplyRecoil()
    {

        float recoilX = Random.Range(recoilXmin, recoilXmax);
        float recoilY = Random.Range(recoilYmin, recoilYmax);
        if (isAiming)
        {
            recoilY = recoilY * 0.5f;
            recoilX = recoilX * 0.5f;
        }
        if (currentBurst >= 7)
        {
            recoilX += 0.15f;
            recoilY += 0.1f;
        }
        cam.transform.localRotation *= originalCameraRotation * Quaternion.Euler(-recoilX, recoilY, 0);
    }

    private void RecoilEnd()
    {
        cam.transform.localRotation = Quaternion.Lerp(cam.transform.localRotation, originalCameraRotation, recoilSmoothness * Time.deltaTime);
    }

    private void AimDownSight()
    {
        Debug.Log("ADS");
        gunAnimator.SetBool("isAiming", true);
        transform.parent.localPosition = Vector3.Lerp(transform.parent.localPosition, adsPos, adsTime);
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 60, adsTime);
    }
    private void HipFire()
    {
        gunAnimator.SetBool("isAiming", false);
        transform.parent.localPosition = Vector3.Lerp(transform.parent.localPosition, originalPos, adsTime);
        cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, 90, adsTime);
    }

    public Vector3 Spread()
    {
        spreadX = Random.Range(spreadMinX, spreadMaxX);
        spreadY = Random.Range(spreadMinY, spreadMaxY);
        if (isAiming)
        {
            spreadY = spreadY * 0.5f;
            spreadX = spreadX * 0.5f;
        }
        spread = new Vector3(spreadX, spreadY, spreadX);
        return spread;
    }

    private void Fire()
    {

        if (ammo > 0 && t_fire >= firing_coolDown && !isReloading && GameManager.Instance.isPaused == false)
        {
            t_fire = 0f;
            ammo--;
            currentBurst++;
            muzzleFX.Play();
            if (!isAiming)
            {
                gunAnimator.Play("Gun_Fire");
            }
            else
            {
                gunAnimator.Play("Gun_AimingFire");
            }

            audioSource.pitch = Random.Range(1.1f, 1.26f);
            audioSource.PlayOneShot(soundEffects[0]);
            if (activateSpread)
            {
                spread = Spread();
            }

            if (Physics.Raycast(cam.transform.position, transform.forward + spread, out RaycastHit hit, range, enemyMask))
            {
                if (hit.transform.CompareTag("Body_Collider"))
                {
                    int rnd = Random.Range(0, GameManager.Instance.bloodFX.Length - 1);
                    Instantiate(GameManager.Instance.bloodFX[rnd], hit.point, transform.rotation);
                    if (GameManager.Instance.instaKill)
                    {
                        hit.transform.parent.gameObject.GetComponent<ZM_AI>().ZM_Death(headShot);
                    }
                    else
                    {
                        hit.transform.parent.gameObject.GetComponent<ZM_AI>().ReduceHP(damage, headShot);
                        headShot = false;
                    }
                }
                if (hit.transform.CompareTag("Head_Collider"))
                {
                    int rnd = Random.Range(0, GameManager.Instance.bloodFX.Length - 1);
                    Instantiate(GameManager.Instance.bloodFX[rnd], hit.point, transform.rotation);
                    if (GameManager.Instance.instaKill)
                    {
                        hit.transform.parent.gameObject.GetComponent<ZM_AI>().ZM_Death(headShot);
                    }
                    else
                    {
                        hit.transform.parent.gameObject.GetComponent<ZM_AI>().ReduceHP(damage * headshotMultiplier, headShot);
                        headShot = true;
                    }
                }
                if (hit.transform.CompareTag("Wall") || hit.transform.CompareTag("Ground"))
                {
                    Instantiate(GameManager.Instance.wallChipFX, hit.point, Quaternion.identity);
                    Instantiate(GameManager.Instance.bulletHoleDecal, hit.point, Quaternion.LookRotation(hit.normal) * Quaternion.Euler(0, 0, Random.Range(0, 360)));
                }
            }
            if (activateRecoil)
            {
                ApplyRecoil();
            }

        }
        if (firingMode != FiringMode.full)
        {
            isFiring = false;

        }

    }

    // Realiza un disparo de perdigones
    private void BuckShot()
    {
        if (ammo > 0 && t_fire >= firing_coolDown && !isReloading && GameManager.Instance.isPaused == false && gameObject != null && transform.parent.gameObject.activeSelf.Equals(true))
        {
            ammo--;
            muzzleFX.Play();
            gunAnimator.Play("Gun_Fire");
            audioSource.pitch = Random.Range(1.1f, 1.26f);
            audioSource.PlayOneShot(soundEffects[0]);
            for (int i = 0; i <= pellets; i++)
            {
                if (activateSpread)
                {
                    spread = Spread();
                }
                if (Physics.Raycast(cam.transform.position, transform.forward + spread, out RaycastHit hit, range, enemyMask))
                {
                    if (hit.transform.CompareTag("Body_Collider"))
                    {
                        int rnd = Random.Range(0, (GameManager.Instance.bloodFX.Length - 1));
                        Instantiate(GameManager.Instance.bloodFX[rnd], hit.point, transform.rotation);
                        if (GameManager.Instance.instaKill)
                        {
                            hit.transform.parent.gameObject.GetComponent<ZM_AI>().ZM_Death(headShot);
                        }
                        else
                        {
                            hit.transform.parent.gameObject.GetComponent<ZM_AI>().ReduceHP(damagePellet, headShot);
                            headShot = false;
                        }
                    }
                    if (hit.transform.CompareTag("Head_Collider"))
                    {
                        int rnd = Random.Range(0, (GameManager.Instance.bloodFX.Length - 1));
                        Instantiate(GameManager.Instance.bloodFX[rnd], hit.point, transform.rotation);
                        if (GameManager.Instance.instaKill)
                        {
                            hit.transform.parent.gameObject.GetComponent<ZM_AI>().ZM_Death(headShot);
                        }
                        else
                        {
                            hit.transform.parent.gameObject.GetComponent<ZM_AI>().ReduceHP(damagePellet * headshotMultiplier, headShot);
                            headShot = true;
                        }
                    }
                    if (hit.transform.CompareTag("Wall") || hit.transform.CompareTag("Ground"))
                    {
                        Instantiate(GameManager.Instance.wallChipFX, hit.point, Quaternion.identity);
                        Instantiate(GameManager.Instance.bulletHoleDecal, hit.point, Quaternion.LookRotation(hit.normal) * Quaternion.Euler(0, 0, Random.Range(0, 360)));

                    }
                }
            }
            t_fire = 0f;
            if (ammo == 0 && reserveAmmo > 0 && t_fire >= firing_coolDown)
            {
                StartCoroutine(ReloadWeapon(emptyReloadTime));
            }
            if (activateRecoil)
            {
                ApplyRecoil();
            }

        }
        if (firingMode != FiringMode.full)
        {
            isFiring = false;

        }
    }


    private void StartShot()
    {
        isFiring = true;
    }

    private void EndShot()
    {
        isFiring = false;
        currentBurst = 0;
    }

    private void StartADS()
    {
        isAiming = true;
    }
    private void EndADS()
    {
        isAiming = false;
    }

    private void Reload()
    {
        if (reserveAmmo > 0 && ammo < ammoCapacity && t_fire >= firing_coolDown && !isAiming && transform.parent.gameObject.activeSelf.Equals(true) && gameObject != null)
        {
            if (ammo <= 0)
            {
                StartCoroutine(ReloadWeapon(emptyReloadTime));
                gunAnimator.Play("Gun_ReloadEmpty");
                gunAnimator.Play("Arms_ReloadEmpty");
            }
            else
            {
                StartCoroutine(ReloadWeapon(reloadTime));
                gunAnimator.Play("Gun_Reload");
                gunAnimator.Play("Arms_Reload");
            }

        }

    }

    // Corrutina para recargar el arma
    IEnumerator ReloadWeapon(float weaponReloadTime)
    {
        isReloading = true;
        yield return new WaitForSeconds(weaponReloadTime);


        // Llena el cargador con munici�n de reserva
        for (int i = ammo; ammo < ammoCapacity && reserveAmmo > 0; i++)
        {
            ammo++;
            reserveAmmo--;
        }
        isReloading = false;


    }

    public void IncreaseReloadSpeed()
    {
        //gunAnimator.speed = 1 * speedMultiplier;
        gunAnimator.SetFloat("speedMultiplier", defaultSpeed * speedMultiplier);
        reloadTime /= speedMultiplier;
        emptyReloadTime /= speedMultiplier;

    }

    public void DefaultReloadSpeed()
    {
        gunAnimator.SetFloat("speedMultiplier", defaultSpeed);
        reloadTime *= speedMultiplier;
        emptyReloadTime *= speedMultiplier;
    }

    void PlaySound(AnimationEvent animEvent)
    {
        audioSource.PlayOneShot(soundEffects[animEvent.intParameter]);
    }
}

