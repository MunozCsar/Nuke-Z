using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
/*
    Este script gestiona las armas del jugador y varias interacciones con diferentes powerups y objetos interactuables, as� como el ataque cuerpo a cuerpo.
*/

public class WeaponHandler : MonoBehaviour
{
    [SerializeField] AudioClip[] meleeSoundFX;
    AudioSource audioSource;
    public LayerMask enemyMask;
    public GameObject bulletOrigin;
    [Header("Melee")]
    public GameObject knife, pickUpWeaponGameObject;
    public float meleeDamage, meleeRange; 
    public float t_melee, melee_coolDown;
    public PlayerController player;
    [Header("Weapons")]
    public Transform gunInstancePosition;
    public List<GameObject> playerWeapons = new();
    public TMP_Text ammoCount, maxAmmoCount;
    bool pickupWeapon = false, ammoBox, isSprinting, isMoving;
    public bool activeWeapon;
    public int activeSlot, weaponSlots;
    int weaponID;

    void Start()
    {
        knife.SetActive(false);
        audioSource = GetComponent<AudioSource>();
        InputManager.Instance.interact += BuyAmmo;
        InputManager.Instance.interact += AddWeaponRef;
        InputManager.Instance.changeWeapon += ChangeWeapon;
        InputManager.Instance.melee += MeleeAttack;
        InputManager.Instance._inputActions.Player.Movement.started += ctx => StartMovement();
        InputManager.Instance._inputActions.Player.Movement.canceled += ctx => EndMovement();
        // InputManager.Instance._inputActions.Player.Sprint.started += ctx => StartSprint();
        // InputManager.Instance._inputActions.Player.Sprint.canceled += ctx => EndSprint();
    }

    void OnEnable(){

    }
    void OnDisable(){
        InputManager.Instance.interact -= BuyAmmo;
        InputManager.Instance.interact -= AddWeaponRef;
        InputManager.Instance.changeWeapon -= ChangeWeapon;
        InputManager.Instance.melee -= MeleeAttack;
        InputManager.Instance._inputActions.Player.Movement.started -= ctx => StartMovement();
        InputManager.Instance._inputActions.Player.Movement.canceled -= ctx => EndMovement();
        // InputManager.Instance._inputActions.Player.Sprint.started -= ctx => StartSprint();
        // InputManager.Instance._inputActions.Player.Sprint.canceled -= ctx => EndSprint();
    }
    // private void StartSprint()
    // {
    //     isSprinting = true;


    // }

    // private void EndSprint()
    // {


    //     isSprinting = false;
    // }
    private void StartMovement()
    {
        isMoving = true;
        if (isSprinting == false && activeWeapon)
        {
            playerWeapons[activeSlot].GetComponentInChildren<Animator>().SetBool("isWalking", true);
        }
        else if(isSprinting == true && activeWeapon)
        {
            playerWeapons[activeSlot].GetComponentInChildren<Animator>().SetBool("isRunning", true);
        }


    }

    private void EndMovement()
    {
        isMoving = false;
        if (activeWeapon)
        {
            playerWeapons[activeSlot].GetComponentInChildren<Animator>().SetBool("isWalking", false);
            playerWeapons[activeSlot].GetComponentInChildren<Animator>().SetBool("isRunning", false);
        }

    }

    void Update()
    {
        if (isSprinting = InputManager.Instance._inputActions.Player.Sprint.ReadValue<float>() > 0f)
        {
        if (isMoving && activeWeapon)
        {
            playerWeapons[activeSlot].GetComponentInChildren<Animator>().SetBool("isRunning", true);
        }
        isSprinting = true;
        }
        else
        {
        if (activeWeapon)
        {
            playerWeapons[activeSlot].GetComponentInChildren<Animator>().SetBool("isRunning", false);
        }
        isSprinting = false;
        }

        if (t_melee < melee_coolDown)
        {
            t_melee += Time.deltaTime;
        }

        if (activeWeapon)
        {
            foreach (GameObject weapon in playerWeapons)
            {
                if (weapon.activeSelf.Equals(true))
                {
                    ammoCount.text = weapon.GetComponentInChildren<GunController>().ammo.ToString();
                    maxAmmoCount.text = "/ " + weapon.GetComponentInChildren<GunController>().reserveAmmo.ToString();
                }
            }

        }
    }
    // Ataque cuerpo a cuerpo
    private void ChangeWeapon()
    {

        if (playerWeapons.Count != 0)
        {
            if (playerWeapons.Count > 1)
            {

                if (activeSlot == 1)
                {
                    activeSlot = 0;           
                    playerWeapons[0].SetActive(true);
                    playerWeapons[1].SetActive(false);
                    Debug.Log("Desactivar controlador");
                }
                else if (activeSlot == 0)
                {
                    activeSlot = 1;
                    playerWeapons[0].SetActive(false);
                    playerWeapons[1].SetActive(true);
                    Debug.Log("Desactivar controlador");
                }
            }
            playerWeapons[activeSlot].GetComponentInChildren<GunController>().isReloading = false;
        }



    }

    private void MeleeAttack()
    {
        if (t_melee >= melee_coolDown)
        {
            knife.SetActive(true);
            if (playerWeapons.Count > 0)
            {
                playerWeapons[activeSlot].SetActive(false);
                playerWeapons[activeSlot].GetComponentInChildren<GunController>().isReloading = false;
            }
            if (Physics.Raycast(bulletOrigin.transform.position, bulletOrigin.transform.forward, out RaycastHit hit, meleeRange, enemyMask))
            {

                if (hit.transform.CompareTag("Body_Collider") || hit.transform.CompareTag("Head_Collider"))
                {
                    audioSource.PlayOneShot(meleeSoundFX[0]);
                    audioSource.PlayOneShot(meleeSoundFX[1]);
                    int rnd = Random.Range(0, GameManager.Instance.bloodFX.Length - 1);
                    Instantiate(GameManager.Instance.bloodFX[rnd], hit.point, transform.rotation);
                    if (GameManager.Instance.instaKill)
                    {
                        hit.transform.parent.gameObject.GetComponent<ZM_AI>().ZM_Death();
                    }
                    else
                    {
                        hit.transform.parent.gameObject.GetComponent<ZM_AI>().ReduceHP(meleeDamage);
                    }

                }
            }
            t_melee = 0f;
        }

    }
    #region Powerups

    public void MaxAmmo() //Municion maxima
    {
        UIManager.Instance.maxAmmoUI.GetComponent<Animator>().Play("MaxAmmo");
        foreach (GameObject weapon in playerWeapons) //Recorre la lista y da al jugador la municion maxima de todas sus armas
        {
            weapon.GetComponentInChildren<GunController>().reserveAmmo = weapon.GetComponentInChildren<GunController>().ammoCapacity * weapon.GetComponentInChildren<GunController>().extraMags;
        }
    }

    public void InstaKill() //Baja Instantanea
    {
        GameManager.Instance.instaKillTimer = 0;
        GameManager.Instance.instaKillCountdown = 30;
        GameManager.Instance.instaKill = true;
    }

    public void DoublePoints() //Puntos Dobles 
    {
        GameManager.Instance.doublePointsTimer = 0;
        GameManager.Instance.doublePointsCountdown = 30;
        GameManager.Instance.doublePoints = true;
    }

    #endregion

    #region Armas y munici�n
    // A�ade el arma espec�ficada al jugador

    private void AddWeaponRef()
    {
        if (pickupWeapon)
        {
            AddWeapon(weaponID);
            pickupWeapon = false;
            Destroy(pickUpWeaponGameObject);
            pickupWeapon = false;
            UIManager.Instance.interactText.gameObject.SetActive(false);
        }

    }
    public void AddWeapon(int weaponID)
    {

        if (playerWeapons.Count == 0) //Si el jugador no tiene armas, a�ade el arma, la activa y sale de la funci�n
        {
            playerWeapons.Add(Instantiate(GameManager.Instance.weaponPrefabs[weaponID], gunInstancePosition.position, gunInstancePosition.rotation, bulletOrigin.transform));
            playerWeapons[0].SetActive(true);
            activeSlot = 0;
            activeWeapon = true;
            return;
        }

        if (playerWeapons.Count == 1) //Si el jugador tiene un arma, a�ade el arma y la activa.
        {
            playerWeapons.Add(Instantiate(GameManager.Instance.weaponPrefabs[weaponID], gunInstancePosition.position, gunInstancePosition.rotation, bulletOrigin.transform));
            playerWeapons[0].SetActive(false);
            playerWeapons[1].SetActive(true);
            activeSlot = 1;
        }

        //Si el jugador tiene 2 armas, detecta la posici�n en la que tiene el arma actualmente y la sustituye por la nueva
        else if (!playerWeapons[0].GetComponentInChildren<GunController>().id.Equals(weaponID) || !playerWeapons[1].GetComponentInChildren<GunController>().id.Equals(weaponID))
        {

            if (activeSlot == 0)
            {
                Destroy(playerWeapons[activeSlot]);
                playerWeapons.RemoveAt(activeSlot);
                playerWeapons.Insert(0, Instantiate(GameManager.Instance.weaponPrefabs[weaponID], gunInstancePosition.position, gunInstancePosition.rotation, bulletOrigin.transform)); //Inserta el nuevo arma en la posici�n dada
            }
            else if (activeSlot == 1)
            {
                Destroy(playerWeapons[activeSlot]);
                playerWeapons.RemoveAt(activeSlot);
                playerWeapons.Insert(1, Instantiate(GameManager.Instance.weaponPrefabs[weaponID], gunInstancePosition.position, gunInstancePosition.rotation, bulletOrigin.transform)); //Inserta el nuevo arma en la posici�n dada
            }
        }
    }

    public void BuyAmmo() //Compra de munici�n
    {
        if (ammoBox && GameManager.Instance.score >= playerWeapons[activeSlot].GetComponentInChildren<GunController>().ammoCost)
        {
        playerWeapons[activeSlot].GetComponentInChildren<GunController>().reserveAmmo = playerWeapons[activeSlot].GetComponentInChildren<GunController>().ammoCapacity * playerWeapons[activeSlot].GetComponentInChildren<GunController>().extraMags;
        GameManager.Instance.ReduceScore(playerWeapons[activeSlot].GetComponentInChildren<GunController>().ammoCost);
        }

    }
    public void ShowWeapon() //Muestra el arma una vez el ataque melee ha acabado
    {
        if (activeWeapon)
        {

            playerWeapons[activeSlot].SetActive(true);
        }

        knife.SetActive(false);
    }
    #endregion

    private void OnTriggerEnter(Collider other) //Detecci�n de triggers
    {
        if (other.CompareTag("PickupWeapon"))
        {
            pickUpWeaponGameObject = other.gameObject;
            pickupWeapon = true;
            weaponID = other.GetComponent<WeaponID>().gunID;
            UIManager.Instance.interactText.gameObject.SetActive(true);
            UIManager.Instance.interactText.GetComponent<Animator>().Play("interact_text_idle");
            UIManager.Instance.interactText.text = "Press \"F\" to pick up " + GameManager.Instance.weaponPrefabs[weaponID].GetComponentInChildren<GunController>().gunName; //Le da informaci�n al jugador sobre la acci�n que va a realizar

        }

        if (other.CompareTag("AmmoBox"))
        {
            ammoBox = true;
            UIManager.Instance.interactText.gameObject.SetActive(true);
            UIManager.Instance.interactText.GetComponent<Animator>().Play("interact_text_idle");
            UIManager.Instance.interactText.text = "Press \"F\" to buy ammo (Cost: 500)";
        }

        if (other.CompareTag("MaxAmmo"))
        {
            MaxAmmo();
            Destroy(other.gameObject);
            Instantiate(GameManager.Instance.powerUp_fx, other.transform.position, other.transform.rotation);
        }
        if (other.CompareTag("Instakill"))
        {
            InstaKill();
            Destroy(other.gameObject);
            Instantiate(GameManager.Instance.powerUp_fx, other.transform.position, other.transform.rotation);
        }
        if (other.CompareTag("DoublePoints"))
        {
            DoublePoints();
            Destroy(other.gameObject);
            Instantiate(GameManager.Instance.powerUp_fx, other.transform.position, other.transform.rotation);
        }
        if (other.CompareTag("NukePowerup"))
        {
            GameManager.Instance.NukePowerUp();
            Destroy(other.gameObject);
            Instantiate(GameManager.Instance.powerUp_fx, other.transform.position, other.transform.rotation);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        UIManager.Instance.interactText.gameObject.SetActive(false);

        if (other.CompareTag("AmmoBox"))
        {
            ammoBox = false;
        }

        if (other.CompareTag("PickupWeapon"))
        {
            pickupWeapon = false;
        }
    }
}


