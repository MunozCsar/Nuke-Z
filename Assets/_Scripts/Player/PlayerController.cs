using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/*
    Este script controla el movimiento, vida y varias interacciones del jugador con el juego y su entorno.
*/


public class PlayerController : MonoBehaviour
{
    public Camera cam;
    public int baseFov;
    public int sprintFov;
    public float fovTime;
    public Transform spawnPoint;

    [Header("Health variables")]
    #region PlayerHealth_Attributes
    public float playerHP;
    public float maxHP;
    public float regenSpeed;
    public float juggHP;
    public float t_regen;
    public float regen_Cooldown;
    public float reviveSpeed;
    public float revive_t;
    public float revive_goal;
    #endregion
    [Header("Perk Manager variables")]
    #region PerkManager_Attributes
    public GameObject perkContainer;
    public PerkID perkAttributes;
    public GameObject[] perkIcons;
    public List<GameObject> player_ActivePerks;
    public int player_PerkCount, activePerk, maxPerks;
    bool perkInteraction;
    public bool quickRevive_Active, juggernog_Active, speedcola_Active;

    #endregion
    [Header("Player Camera")]
    #region Player Camera
    public Camera playerCam;
    public Transform cameraDown_Position, cameraUp_Position;
    public float downSpeed;
    PlayerDowned down_script;
    #endregion

    public WeaponHandler weaponHandler;

    private AudioSource audioSource;
    [Header("Actions variables")]
    #region PlayerActions_Attributes
    public GameObject flashLight;
    public float t_barrier, barrier_Cooldown;
    private bool performInteraction = false, interacting = false, flashOn;
    private GameObject currentBarrier;
    #endregion

    [Header("Movement variables")]
    #region PlayerMovement_Attributes
    Vector3 velocity;
    public bool isSprinting, isMoving;
    public float gravity = Physics.gravity.y * 2;
    public float walkSpeed, strafeSpeed, adsSpeed;
    public float sprintMultiplier;
    public float jumpHeight;
    int footstep = 0;
    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;
    private float footstepDelay;
    public float footstepDelayGoal;
    [SerializeField] AudioClip[] footStepArray;
    public CharacterController controller;
    private bool isGrounded, end_Game;
    public bool isDowned;
    #endregion
    private void Start()
    {
        down_script = GetComponent<PlayerDowned>();
        audioSource = GetComponent<AudioSource>();
        gameObject.transform.position = spawnPoint.position;
        InputManager.Instance._inputActions.Player.Interact.started += ctx => StartInteraction();
        InputManager.Instance._inputActions.Player.Interact.canceled += ctx => EndInteraction();
        // InputManager.Instance._inputActions.Player.Sprint.started += ctx => StartSprint();
        // InputManager.Instance._inputActions.Player.Sprint.canceled += ctx => EndSprint();
        InputManager.Instance._inputActions.Player.Movement.started += ctx => StartMovement();
        InputManager.Instance._inputActions.Player.Movement.canceled += ctx => EndMovement();
        InputManager.Instance.flashLight += TurnOnFlashLight;
        InputManager.Instance.jump += Jump;
        InputManager.Instance.interact += EndGame;
    }

    private void Update()
    {
        isSprinting = InputManager.Instance._inputActions.Player.Sprint.ReadValue<float>() > 0f;
        if (weaponHandler.playerWeapons.Count != 0)
        {
            if (weaponHandler.playerWeapons[weaponHandler.activeSlot].GetComponentInChildren<GunController>().isAiming)
            {
                isSprinting = false;
                MovePlayer(adsSpeed, isSprinting);
            }
            else
            {
                MovePlayer(walkSpeed, isSprinting);
            }
        }
            else
            {
                MovePlayer(walkSpeed, isSprinting);
            }

        if (!isSprinting || !isMoving)
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, baseFov, fovTime);
        }
        else if (isMoving && isSprinting)
        {
            cam.fieldOfView = Mathf.Lerp(cam.fieldOfView, sprintFov, fovTime);
        }

        if (footstepDelay < footstepDelayGoal)
        {
            footstepDelay += Time.deltaTime;
        }
        Revive();
        CheckGround();
        ApplyGravity();
        RegenHP();
        RepairBarrier();
        if (perkAttributes != null)
        {
            ObtainPerk(perkAttributes.id, perkAttributes.cost);
        }
        if (isDowned)
        {
            playerCam.transform.position = Vector3.Lerp(playerCam.transform.position, cameraDown_Position.position, downSpeed * Time.deltaTime);
        }
        else if (!isDowned && Vector3.Distance(playerCam.transform.position, cameraUp_Position.position) > 0.01f)
        {
            playerCam.transform.position = Vector3.Lerp(playerCam.transform.position, cameraUp_Position.position, downSpeed * Time.deltaTime);
        }

        controller.Move(velocity * Time.deltaTime);

    }
    #region Player Movement
    private void StartMovement()
    {
        isMoving = true;
    }

    private void EndMovement()
    {
        isMoving = false;
    }
    // private void StartSprint()
    // {
    //     isSprinting = true;
    // }

    // private void EndSprint()
    // {
    //     isSprinting = false;
    // }
    private void EndGame()
    {
        if (end_Game.Equals(true))
        {
            if (nz_factory_manager.Instance.endGameTrigger)
            {
                GameManager.Instance.EndGame();
            }
        }
    }
    private void CheckGround()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask); //Se comprueba si el jugador est� en tierra usando un CheckSphere
    }
    private void ApplyGravity()
    {
        if (isGrounded && velocity.y < 0)
        {
            velocity.y = -5;
        }
        velocity.y += gravity * Time.deltaTime;
    }
    //Funci�n de movimiento del jugador
    private void MovePlayer(float speed, bool isRunning)
    {
        float x = InputManager.Instance.GetMovementDirection().x;
        float z = InputManager.Instance.GetMovementDirection().y;

        Vector3 move = transform.right * x + transform.forward * z;
        if (isDowned)
        {
            speed = 3;
            isRunning = false;
        }
        if (!isRunning && isMoving)
        {
            controller.Move(speed * Time.deltaTime * move.normalized);
            if (footstepDelay >= footstepDelayGoal)
            {
                audioSource.PlayOneShot(footStepArray[footstep]);
                footstepDelay = 0;
                if (footstep == 0)
                {
                    footstep = 1;
                }
                else if (footstep == 1)
                {
                    footstep = 0;
                }
            }
        }
        else if (isMoving)
        {
            controller.Move(speed * sprintMultiplier * Time.deltaTime * move.normalized);

            if (footstepDelay >= footstepDelayGoal / sprintMultiplier)
            {
                audioSource.PlayOneShot(footStepArray[footstep]);
                footstepDelay = 0;
                if (footstep == 0)
                {
                    footstep = 1;
                }
                else if (footstep == 1)
                {
                    footstep = 0;
                }
            }
        }


    }

    private void Jump()
    {
        if (isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }
    }
    #endregion
    #region Player Actions
    private void TurnOnFlashLight()
    {

        if (!flashOn)
        {
            flashLight.SetActive(true);
            flashOn = true;
        }
        else if (flashOn)
        {
            flashLight.SetActive(false);
            flashOn = false;
        }
    }

    private void StartInteraction()
    {
        performInteraction = true;
    }

    private void EndInteraction()
    {
        performInteraction = false;
    }


    public void ClearDebris(GameObject debris){
        if (performInteraction && interacting){
            debris.GetComponent<Animator>().SetTrigger("clearDebris");
            GameManager.Instance.ReduceScore(debris.GetComponent<Debris>().debrisCost);
        }
    }
    public void RepairBarrier()
    {
        if (performInteraction && interacting)
        {
            if (t_barrier < barrier_Cooldown)
            {
                t_barrier += Time.deltaTime;
            }
            else if (t_barrier >= barrier_Cooldown)
            {
                t_barrier = 0f;
                currentBarrier.GetComponent<BarrierLogic>().RepairBarrier();
            }
        }
    }
    #endregion
    #region Triggers
    private void OnTriggerEnter(Collider other)
    {
        //Gesti�n de triggers para activar y desactivar eventos interactivos

        if (other.CompareTag("Damage_Trigger")) //Recibe da�o al ser golpeado
        {
            GameManager.Instance.ShowDamageIndicators(playerHP);
            if (playerHP > 0)
            {
                TakeDamage(GameManager.Instance.zm_Damage);
            }
        }
        if (other.CompareTag("EndGameTrigger")) //Comprobaci�n de poder acabar la partida
        {
            UIManager.Instance.interactText.gameObject.SetActive(true);
            UIManager.Instance.interactText.GetComponent<Animator>().Play("interact_text_idle");
            if (nz_factory_manager.Instance.endGameTrigger)
            {
                UIManager.Instance.interactText.text = "Press \"F\" to turn on reactor";
                end_Game = true;
            }
            else
            {
                UIManager.Instance.interactText.text = "Fill up the containers to turn on the reactor";
            }


        }
        if (other.CompareTag("PerkMachine"))
        {
            perkInteraction = true;
            //activePerk = other.gameObject.GetComponent<PerkID>().id;
            perkAttributes = other.gameObject.GetComponent<PerkID>();
            UIManager.Instance.interactText.gameObject.SetActive(true);
            UIManager.Instance.interactText.text = "Press F to buy perk (Cost: " + perkAttributes.cost + " points)";
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //Desactiva las bools al dejar de hace contacto con ellas

        if (other.CompareTag("EndGameTrigger"))
        {
            end_Game = false;
        }
        if (other.CompareTag("Barrier_Trigger"))
        {
            interacting = false;
            t_barrier = 0;
        }
        if (other.CompareTag("PerkMachine"))
        {
            perkInteraction = false;
        }
    }

    // Gesti�n de triggers de tipo stay
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Barrier_Trigger"))
        {
            currentBarrier = other.transform.parent.gameObject;
            interacting = true;
            if (other.transform.parent.GetComponent<BarrierLogic>().hitPoints < 9)
            {
                UIManager.Instance.interactText.gameObject.SetActive(true);
                UIManager.Instance.interactText.GetComponent<Animator>().Play("interact_text_idle");
                UIManager.Instance.interactText.text = "Press \"F\" to repair barrier";


            }
            else if (other.transform.parent.GetComponent<BarrierLogic>().hitPoints >= 9)
            {
                UIManager.Instance.interactText.gameObject.SetActive(false);
                interacting = false;
            }
        }
    
        if(other.CompareTag("Debris") && GameManager.Instance.score >= other.GetComponent<Debris>().debrisCost)
        {
            interacting = true;
            GameObject debris = other.gameObject;
            UIManager.Instance.interactText.gameObject.SetActive(true);
            UIManager.Instance.interactText.text = "Press F to clear debris (Cost: " + debris.GetComponent<Debris>().debrisCost + " points)";
            ClearDebris(debris);
        }
    }
    #endregion
    #region Player Health & Damage
    public void TakeDamage(float damage) //Recibir da�o
    {
        t_regen = 0f;
        playerHP -= Mathf.RoundToInt(damage);
        if (playerHP <= 0 && !quickRevive_Active)
        {
            GameManager.Instance.GameOver(this.gameObject);
            isDowned = true;
            if (GetComponentInChildren<WeaponHandler>().activeWeapon)
            {
                GetComponentInChildren<WeaponHandler>().playerWeapons[GetComponentInChildren<WeaponHandler>().activeSlot].SetActive(false);
            }

        }
        else if (playerHP <= 0 && quickRevive_Active)
        {
            OnPlayerDowned();
        }
    }
    public void OnPlayerDowned()
    {
        revive_t = 0;
        gameObject.tag = "DownedPlayer";
        isDowned = true;
        PlayerDown();
        Debug.Log("Player Down");
        for (int i = 0; i < maxPerks; i++)
        {
            LosePerk(i);
        }
    }

    public void PlayerDown()
    {
        if (isDowned)
        {
            playerCam.transform.position = Vector3.Lerp(playerCam.transform.position, cameraDown_Position.position, downSpeed * Time.deltaTime);
        }
    }
    public void Revive()
    {
        if (revive_t < revive_goal)
        {
            revive_t += Time.deltaTime * reviveSpeed;
        }
        else if (revive_t >= revive_goal)
        {
            isDowned = false;
            gameObject.tag = "Player";
        }
    }
    public void RegenHP() //Regeneraci�n de vida
    {
        if (juggernog_Active)
        {
            if (t_regen < regen_Cooldown / 1.5)
            {
                t_regen += Time.deltaTime;
            }
            else
            {
                if (playerHP < juggHP) //Si el jugador est� por debajo del valor m�ximo de vida, se regenera la vida()
                {
                    playerHP += regenSpeed * 2 * Time.deltaTime;
                }

            }
        }
        else
        {
            if (t_regen < regen_Cooldown)
            {
                t_regen += Time.deltaTime;
            }
            else
            {
                if (playerHP < maxHP) //Si el jugador est� por debajo del valor m�ximo de vida, se regenera la vida()
                {
                    playerHP += regenSpeed * Time.deltaTime;
                }

            }
        }



    }
    #endregion
    #region Perk Manager
    public void ObtainPerk(int perkID, int perkCost)
    {
        if (perkInteraction && performInteraction && GameManager.Instance.score >= perkCost)
        {
            switch (perkID)
            {
                case 0:
                if (!quickRevive_Active)
                    {
                        //Quick revive
                        quickRevive_Active = true;
                        player_PerkCount++;
                        player_ActivePerks.Add(Instantiate(perkIcons[perkID], perkContainer.transform));
                        GameManager.Instance.ReduceScore(perkCost);
     
                    }
                    break;
                case 1:
                    
                    if (!juggernog_Active)
                    {
                        //Juggernog
                        juggernog_Active = true;
                        player_PerkCount++;
                        player_ActivePerks.Add(Instantiate(perkIcons[perkID], perkContainer.transform));
                        GameManager.Instance.ReduceScore(perkCost);
                    }
                    break;
                case 2:
                    if (!speedcola_Active)
                    {
                        //Quick revive
                        speedcola_Active = true;
                        player_PerkCount++;
                        player_ActivePerks.Add(Instantiate(perkIcons[perkID], perkContainer.transform));
                        foreach (GameObject gun in weaponHandler.playerWeapons)
                        {
                            gun.GetComponentInChildren<GunController>().IncreaseReloadSpeed();
                            Debug.Log("debug_speed cola " + gun.name);
                        }
                        GameManager.Instance.ReduceScore(perkCost);
                    }
                    break;
            }

        }

    }
    public void LosePerk(int perkID)
    {
        switch (perkID)
        {
            case 0:
                if (juggernog_Active) { juggernog_Active = false; }
                break;
            case 1:
                if (quickRevive_Active) { quickRevive_Active = false; }
                break;
            case 2:
                if (speedcola_Active)
                {
                    speedcola_Active = false;
                    foreach (GameObject gun in weaponHandler.playerWeapons)
                    {
                        gun.GetComponentInChildren<GunController>().DefaultReloadSpeed();
                    }
                }
                break;
            case 3: break;
            default: break;
        }
        foreach (GameObject perk in player_ActivePerks)
        {
            player_PerkCount--;
            Destroy(perk);

        }
        player_ActivePerks.Clear();
    }
    #endregion

}
